using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Exceptions;
using Oip.Base.Services;
using Oip.Hitl.Base.Controllers.Api;
using Oip.Hitl.Base.Settings;
using Oip.Hitl.Base.Workflows;
using Oip.Hitl.Controllers.Api;
using Oip.Hitl.Services;
using Oip.Hitl.Settings;
using Oip.Hitl.Workflows;
using Temporalio.Client;
using Temporalio.Exceptions;

namespace Oip.Hitl.Controllers;

/// <summary>
/// OpenAI-compatible API for chat UIs such as Open WebUI: each chat message starts an <see cref="AgentWorkflow"/>
/// and its answer is returned, or streamed as server-sent events. Routes and errors follow the OpenAI API instead of
/// the OIP conventions, and the controller is not in the generated web client. When the run waits for the user, the
/// step is streamed too and the user completes it with <see cref="CompleteStep"/>, or on the task page of OIP.
/// </summary>
[ApiController]
[Authorize]
[OpenAiExceptionFilter]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("v1")]
public class AgentGatewayController(
    AgentService agentService,
    ITemporalClient client,
    TemporalSettings temporalSettings,
    AgentGatewaySettings gatewaySettings,
    AgentEventStream eventStream,
    UserStepService userStepService,
    ClaimService claimService,
    ILogger<AgentGatewayController> logger) : ControllerBase
{
    private const string OwnedBy = "oip";
    private const string AssistantRole = "assistant";
    private const string RetrySeparator = "\n\n---\n\n";
    private const string TurnSeparator = "\n\n";
    private const string RunIdPrefix = "agent-";
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Returns the models the chat can use: the enabled agents.
    /// </summary>
    [HttpGet("models")]
    public async Task<IActionResult> GetModels(CancellationToken cancellationToken)
    {
        var agents = await agentService.GetEnabledAgentsAsync(cancellationToken);
        var models = agents
            .Select(x => new OpenAiModel(x.Code, ToUnixSeconds(x.CreatedAt), OwnedBy, x.Name))
            .ToList();
        return new JsonResult(new OpenAiModelList(models), OpenAiJson.Options);
    }

    /// <summary>
    /// Starts an agent run for the chat and returns its answer, or streams it when <c>stream</c> is set. The run is
    /// cancelled when the client disconnects, e.g. when the user stops the generation.
    /// </summary>
    [HttpPost("chat/completions")]
    public async Task<IActionResult> CreateChatCompletion([FromBody] ChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var messages = OpenAiChatConverter.ToAgentMessages(request.Messages);
        var settings = OpenAiChatConverter.ToSettings(request.Parameters);
        var agent = await FindAgentAsync(request.Model, cancellationToken);

        var runId = $"{RunIdPrefix}{Guid.NewGuid():N}";
        var streamKey = request.Stream ? AgentEventStream.GetKey(runId) : null;
        var userName = claimService.GetUserLogin();
        var input = new AgentWorkflowInput(agent, messages, settings, streamKey, userName,
            TimeSpan.FromMinutes(gatewaySettings.UserStepTimeoutMinutes));
        var handle = await client.CallAsync(() => client.StartWorkflowAsync(
            (AgentWorkflow workflow) => workflow.RunAsync(input),
            new WorkflowOptions(runId, temporalSettings.TaskQueue)
            {
                ExecutionTimeout = TimeSpan.FromMinutes(gatewaySettings.RunTimeoutMinutes),
                Memo = userName is null ? null : new Dictionary<string, object> { [AgentWorkflow.UserMemo] = userName },
                Rpc = new RpcOptions { CancellationToken = cancellationToken }
            }));

        var completionId = $"chatcmpl-{runId}";
        var created = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        try
        {
            if (streamKey is not null)
            {
                await StreamAsync(handle, streamKey, completionId, created, agent.Code, request.OipEvents,
                    cancellationToken);
                return new EmptyResult();
            }

            var result = await GetResultAsync(handle, cancellationToken);
            return new JsonResult(new ChatCompletion(completionId, created, agent.Code,
                [new ChatCompletionChoice(0, new ChatCompletionResponseMessage(AssistantRole, result.Content),
                    result.FinishReason ?? "stop")],
                ToUsage(result)), OpenAiJson.Options);
        }
        catch (Exception) when (cancellationToken.IsCancellationRequested)
        {
            // The client disconnected; Temporal reports it as a cancellation or as a cancelled RPC.
            await CancelAsync(handle);
            return new EmptyResult();
        }
        catch (Exception e) when (Response.HasStarted)
        {
            // The answer cannot be completed, e.g. Redis is unavailable: the connection is aborted.
            logger.LogError(e, "Streaming of agent run {WorkflowId} failed", handle.Id);
            await CancelAsync(handle);
            throw;
        }
    }

    /// <summary>
    /// Completes a step the agent run waits for, e.g. answers its question. Only the user who started the run
    /// completes its steps here.
    /// </summary>
    [HttpPost("agent-runs/{runId}/steps/{stepId}/complete")]
    public async Task<IActionResult> CompleteStep(string runId, string stepId,
        [FromBody] CompleteAgentStepRequest request, CancellationToken cancellationToken)
    {
        var userName = claimService.GetUserLogin();
        if (!runId.StartsWith(RunIdPrefix, StringComparison.Ordinal) ||
            await GetRunUserAsync(runId, cancellationToken) is not { } owner ||
            !string.Equals(owner, userName, StringComparison.OrdinalIgnoreCase))
            throw new ApiException("Not found", "Step not found or already completed", StatusCodes.Status404NotFound);

        await userStepService.CompleteAsync(runId, stepId, new CompleteUserStepRequest
        {
            Result = JsonSerializer.Serialize(request.Result?.Trim() ?? string.Empty),
            Comment = request.Comment
        }, userName, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Writes the events of the run as chunks until the workflow is closed. The workflow closes after its activities
    /// published all their events, so the stream is read once more after that and then ends. The texts of the turns
    /// are separated like in the answer of the workflow. With <paramref name="oipEvents"/> statuses and user steps
    /// are sent as <see cref="AgentRunEvent"/>s; otherwise statuses, e.g. tool calls, are sent as reasoning, which
    /// chat UIs show apart from the answer, and a user step as text with the link to its page.
    /// </summary>
    private async Task StreamAsync(WorkflowHandle<AgentWorkflow, AgentTurnResult> handle, string streamKey,
        string completionId, long created, string model, bool oipEvents, CancellationToken cancellationToken)
    {
        Response.StatusCode = StatusCodes.Status200OK;
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no";
        HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

        ChatCompletionChunk Chunk(ChatCompletionDelta delta, string? finishReason = null,
            ChatCompletionUsage? usage = null) =>
            new(completionId, created, model, [new ChatCompletionChunkChoice(0, delta, finishReason)], usage);

        ChatCompletionChunk EventChunk(AgentRunEvent runEvent) => new(completionId, created, model, [], Oip: runEvent);

        await WriteEventAsync(Chunk(new ChatCompletionDelta(AssistantRole, "")), cancellationToken);

        var resultTask = GetResultAsync(handle, cancellationToken);
        // The result is not awaited when the stream fails; its exception must not go unobserved then.
        _ = resultTask.ContinueWith(task => task.Exception, TaskContinuationOptions.OnlyOnFaulted);
        var position = AgentEventStream.Beginning;
        var hasText = false;
        var turnStarted = false;
        while (true)
        {
            var closed = resultTask.IsCompleted;
            var events = await eventStream.ReadAsync(streamKey, position);
            foreach (var agentEvent in events)
            {
                position = agentEvent.Id;
                ChatCompletionDelta? delta = null;
                switch (agentEvent.Type)
                {
                    case AgentEventType.Start:
                        // A retried attempt repeats the text of its turn after a separator.
                        if (agentEvent.Attempt > 1 && turnStarted)
                            delta = new ChatCompletionDelta(Content: RetrySeparator);
                        else
                            turnStarted = false;
                        break;
                    case AgentEventType.Status when oipEvents:
                        await WriteEventAsync(EventChunk(new AgentRunEvent("status", agentEvent.Text)),
                            cancellationToken);
                        break;
                    case AgentEventType.Status:
                        delta = new ChatCompletionDelta(ReasoningContent: agentEvent.Text + "\n");
                        break;
                    case AgentEventType.UserStep when agentEvent.StepId is not null:
                        var step = await FindStepAsync(handle.Id, agentEvent.StepId, cancellationToken);
                        if (step is null) break;
                        if (oipEvents)
                        {
                            await WriteEventAsync(EventChunk(ToRunEvent(step, agentEvent.StepKind)), cancellationToken);
                            break;
                        }

                        delta = new ChatCompletionDelta(Content: (hasText ? TurnSeparator : "") + FormatStep(step));
                        hasText = true;
                        turnStarted = false;
                        break;
                    case AgentEventType.Delta when !string.IsNullOrEmpty(agentEvent.Text):
                        var text = !turnStarted && hasText ? TurnSeparator + agentEvent.Text : agentEvent.Text;
                        hasText = turnStarted = true;
                        delta = new ChatCompletionDelta(Content: text);
                        break;
                }

                if (delta is not null)
                    await WriteEventAsync(Chunk(delta), cancellationToken);
            }

            if (events.Count > 0) continue;
            if (closed) break;
            await Task.WhenAny(resultTask, Task.Delay(PollInterval, cancellationToken));
            cancellationToken.ThrowIfCancellationRequested();
        }

        try
        {
            var result = await resultTask;
            await WriteEventAsync(Chunk(new ChatCompletionDelta(), result.FinishReason ?? "stop", ToUsage(result)),
                cancellationToken);
        }
        catch (ApiException e)
        {
            // The status is already sent, so the error is reported in the stream.
            await WriteEventAsync(new OpenAiErrorResponse(OpenAiExceptionFilterAttribute.ToError(e).Error),
                cancellationToken);
        }

        await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Waits for the answer of the run; a failed run is reported as <see cref="ApiException"/> with the cause.
    /// </summary>
    private async Task<AgentTurnResult> GetResultAsync(WorkflowHandle<AgentWorkflow, AgentTurnResult> handle,
        CancellationToken cancellationToken)
    {
        try
        {
            return await client.CallAsync(() =>
                handle.GetResultAsync(rpcOptions: new RpcOptions { CancellationToken = cancellationToken }));
        }
        catch (WorkflowFailedException e)
        {
            logger.LogWarning(e, "Agent run {WorkflowId} failed", handle.Id);
            throw new ApiException("Agent run failed", GetCause(e), StatusCodes.Status502BadGateway);
        }
    }

    /// <summary>
    /// Returns the login of the user who started the agent run; <c>null</c> when there is no such run.
    /// </summary>
    private Task<string?> GetRunUserAsync(string runId, CancellationToken cancellationToken)
    {
        return client.CallAsync(async () =>
        {
            try
            {
                var execution = await client.GetWorkflowHandle(runId).DescribeAsync(
                    new WorkflowDescribeOptions { Rpc = new RpcOptions { CancellationToken = cancellationToken } });
                return execution.Memo.TryGetValue(AgentWorkflow.UserMemo, out var memo)
                    ? await memo.ToValueAsync<string>()
                    : null;
            }
            catch (RpcException e) when (e.Code is RpcException.StatusCode.NotFound
                                             or RpcException.StatusCode.InvalidArgument)
            {
                return null;
            }
        });
    }

    /// <summary>
    /// Reads the step the run waits for; <c>null</c> when it cannot be read, so it is answered on its page only.
    /// </summary>
    private async Task<UserStepDto?> FindStepAsync(string runId, string stepId, CancellationToken cancellationToken)
    {
        try
        {
            return await userStepService.GetByIdAsync(runId, stepId, cancellationToken);
        }
        catch (ApiException e)
        {
            logger.LogWarning("Step {StepId} of agent run {WorkflowId} is not found: {Error}", stepId, runId,
                e.Message);
            return null;
        }
    }

    private static AgentRunEvent ToRunEvent(UserStepDto step, string? kind) =>
        new("user_step", RunId: step.WorkflowInstanceId, StepId: step.Id, Kind: kind ?? AgentStepKind.Question,
            Title: step.Title, Description: step.Description, Outcomes: GetOutcomes(step), Url: step.Url);

    /// <summary>
    /// Text of a user step for chat UIs without the pipe: a quote with the link to its page.
    /// </summary>
    private static string FormatStep(UserStepDto step)
    {
        var text = new StringBuilder($"> ⏸ **{step.Title}**\n>\n");
        if (!string.IsNullOrWhiteSpace(step.Description))
        {
            foreach (var line in step.Description.Split('\n'))
                text.Append($"> {line.TrimEnd('\r')}\n");
            text.Append(">\n");
        }

        if (GetOutcomes(step) is { Count: > 0 } outcomes)
            text.Append($"> {string.Join(" / ", outcomes)}\n>\n");
        text.Append($"> [Answer]({step.Url})");
        return text.ToString();
    }

    private static List<string> GetOutcomes(UserStepDto step)
    {
        if (step.Data is null) return [];
        using var data = JsonDocument.Parse(step.Data);
        return data.RootElement.ValueKind == JsonValueKind.Object &&
               data.RootElement.TryGetProperty("outcomes", out var outcomes) &&
               outcomes.ValueKind == JsonValueKind.Array
            ? outcomes.EnumerateArray().Select(x => x.GetString()).OfType<string>().ToList()
            : [];
    }

    private async Task CancelAsync(WorkflowHandle handle)
    {
        try
        {
            await handle.CancelAsync();
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to cancel agent run {WorkflowId}", handle.Id);
        }
    }

    private async Task<AgentSnapshot> FindAgentAsync(string? model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ApiException("Invalid request", "model is required", StatusCodes.Status400BadRequest);

        return await agentService.GetSnapshotAsync(model.Trim(), cancellationToken)
               ?? throw new ApiException("Not found", $"The model '{model}' does not exist",
                   StatusCodes.Status404NotFound);
    }

    private async Task WriteEventAsync(object data, CancellationToken cancellationToken)
    {
        await Response.WriteAsync($"data: {JsonSerializer.Serialize(data, OpenAiJson.Options)}\n\n",
            cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Message of the innermost failure, e.g. the error of the provider instead of "Activity task failed".
    /// </summary>
    private static string GetCause(Exception exception)
    {
        var cause = exception;
        while (cause.InnerException is FailureException inner)
            cause = inner;
        return cause.Message;
    }

    private static ChatCompletionUsage? ToUsage(AgentTurnResult result) =>
        result is { PromptTokens: { } prompt, CompletionTokens: { } completion }
            ? new ChatCompletionUsage(prompt, completion, prompt + completion)
            : null;

    private static long ToUnixSeconds(DateTime value) =>
        new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)).ToUnixTimeSeconds();
}
