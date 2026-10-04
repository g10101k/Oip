using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Exceptions;
using Oip.Base.Services;
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
/// the OIP conventions, and the controller is not in the generated web client.
/// </summary>
[ApiController]
[Authorize]
[OpenAiExceptionFilter]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("v1")]
public class AgentGatewayController(
    LlmProviderService providerService,
    ITemporalClient client,
    TemporalSettings temporalSettings,
    AgentGatewaySettings gatewaySettings,
    AgentEventStream eventStream,
    ClaimService claimService,
    ILogger<AgentGatewayController> logger) : ControllerBase
{
    private const string OwnedBy = "oip";
    private const string AssistantRole = "assistant";
    private const string RetrySeparator = "\n\n---\n\n";
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Returns the models the chat can use: the enabled LLM providers.
    /// </summary>
    [HttpGet("models")]
    public async Task<IActionResult> GetModels(CancellationToken cancellationToken)
    {
        var providers = await providerService.GetAllAsync(cancellationToken);
        var models = providers
            .Where(x => x.IsEnabled)
            .Select(x => new OpenAiModel(x.Name, ToUnixSeconds(x.CreatedAt), OwnedBy))
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
        var provider = await FindProviderAsync(request.Model, cancellationToken);

        var runId = $"agent-{Guid.NewGuid():N}";
        var streamKey = request.Stream ? AgentEventStream.GetKey(runId) : null;
        var input = new AgentWorkflowInput(provider.Id, messages, settings, streamKey, claimService.GetUserLogin());
        var handle = await client.CallAsync(() => client.StartWorkflowAsync(
            (AgentWorkflow workflow) => workflow.RunAsync(input),
            new WorkflowOptions(runId, temporalSettings.TaskQueue)
            {
                ExecutionTimeout = TimeSpan.FromMinutes(gatewaySettings.RunTimeoutMinutes),
                Rpc = new RpcOptions { CancellationToken = cancellationToken }
            }));

        var completionId = $"chatcmpl-{runId}";
        var created = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        try
        {
            if (streamKey is not null)
            {
                await StreamAsync(handle, streamKey, completionId, created, provider.Name, cancellationToken);
                return new EmptyResult();
            }

            var result = await GetResultAsync(handle, cancellationToken);
            return new JsonResult(new ChatCompletion(completionId, created, provider.Name,
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
    /// Writes the events of the run as chunks until the workflow is closed. The workflow closes after its activities
    /// published all their events, so the stream is read once more after that and then ends.
    /// </summary>
    private async Task StreamAsync(WorkflowHandle<AgentWorkflow, AgentTurnResult> handle, string streamKey,
        string completionId, long created, string model, CancellationToken cancellationToken)
    {
        Response.StatusCode = StatusCodes.Status200OK;
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no";
        HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

        ChatCompletionChunk Chunk(ChatCompletionDelta delta, string? finishReason = null,
            ChatCompletionUsage? usage = null) =>
            new(completionId, created, model, [new ChatCompletionChunkChoice(0, delta, finishReason)], usage);

        await WriteEventAsync(Chunk(new ChatCompletionDelta(AssistantRole, "")), cancellationToken);

        var resultTask = GetResultAsync(handle, cancellationToken);
        // The result is not awaited when the stream fails; its exception must not go unobserved then.
        _ = resultTask.ContinueWith(task => task.Exception, TaskContinuationOptions.OnlyOnFaulted);
        var position = AgentEventStream.Beginning;
        var hasText = false;
        while (true)
        {
            var closed = resultTask.IsCompleted;
            var events = await eventStream.ReadAsync(streamKey, position);
            foreach (var agentEvent in events)
            {
                position = agentEvent.Id;
                var text = agentEvent.Type == AgentEventType.Start
                    ? agentEvent.Attempt > 1 && hasText ? RetrySeparator : null
                    : agentEvent.Text;
                if (string.IsNullOrEmpty(text)) continue;

                hasText = true;
                await WriteEventAsync(Chunk(new ChatCompletionDelta(Content: text)), cancellationToken);
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

    private async Task<LlmProviderDto> FindProviderAsync(string? model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ApiException("Invalid request", "model is required", StatusCodes.Status400BadRequest);

        var providers = await providerService.GetAllAsync(cancellationToken);
        return providers.FirstOrDefault(x => x.IsEnabled && string.Equals(x.Name, model.Trim(),
                   StringComparison.OrdinalIgnoreCase))
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
