using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Oip.Base.Exceptions;
using Oip.Base.Settings;
using Oip.Hil.Base.Controllers.Api;
using Oip.Hil.Base.Settings;
using Temporalio.Api.Enums.V1;
using Temporalio.Client;
using Temporalio.Exceptions;

namespace Oip.Hil.Base.Workflows;

/// <summary>
/// Reads <see cref="UserStep"/>s from the <see cref="UserWorkflowBase.StepsMemo"/> memo of the user workflows and
/// completes them through the workflow update.
/// </summary>
public class UserStepService(
    ITemporalClient client,
    TemporalSettings temporalSettings,
    OipWorkflowOptions workflowOptions,
    ISettings settings)
{
    private static readonly TimeSpan FirstStepTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan FirstStepPollInterval = TimeSpan.FromMilliseconds(200);

    private readonly string? _workflowsQuery =
        CreateWorkflowsQuery(temporalSettings.TaskQueue, workflowOptions.UserWorkflowNames);

    /// <summary>
    /// Returns all steps, pending and completed, of the user workflows of the application task queue started in
    /// the period, newest first.
    /// </summary>
    public Task<List<UserStepDto>> GetStepsAsync(DateTimeOffset from, DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        if (_workflowsQuery is null) return Task.FromResult(new List<UserStepDto>());

        var query = $"{_workflowsQuery} AND StartTime >= {Quote(FormatTime(from))} AND StartTime <= {Quote(FormatTime(to))}";
        return client.CallAsync(async () =>
        {
            var result = new List<UserStepDto>();
            var options = new WorkflowListOptions { Rpc = new RpcOptions { CancellationToken = cancellationToken } };
            await foreach (var execution in client.ListWorkflowsAsync(query, options)
                               .WithCancellation(cancellationToken))
            {
                var steps = await ReadStepsAsync(execution);
                result.AddRange(steps.Select(step => ToDto(step, execution)));
            }

            return result.OrderByDescending(x => x.CreatedAt).ToList();
        });
    }

    /// <summary>
    /// Returns a step of the workflow by its id, pending or completed.
    /// </summary>
    public Task<UserStepDto> GetByIdAsync(string workflowId, string stepId, CancellationToken cancellationToken)
    {
        return client.CallAsync(async () =>
        {
            var execution = await TryDescribeAsync(workflowId, cancellationToken) ?? throw StepNotFound();
            var steps = await ReadStepsAsync(execution);
            var step = steps.FirstOrDefault(x => x.Id == stepId) ?? throw StepNotFound();
            return ToDto(step, execution);
        });
    }

    /// <summary>
    /// Completes a step; the workflow validates the result and rejects an invalid one.
    /// </summary>
    public Task<CompleteUserStepResponse> CompleteAsync(string workflowId, string stepId,
        CompleteUserStepRequest request, string? completedBy, CancellationToken cancellationToken)
    {
        return client.CallAsync(async () =>
        {
            var handle = client.GetWorkflowHandle(workflowId);
            var rpc = new RpcOptions { CancellationToken = cancellationToken };
            var completion = new UserStepCompletion
            {
                StepId = stepId,
                Result = ParseResult(request.Result),
                Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
                CompletedBy = completedBy
            };

            try
            {
                await handle.ExecuteUpdateAsync(UserWorkflowBase.CompleteStepUpdate, [completion],
                    new WorkflowUpdateOptions { Rpc = rpc });
            }
            catch (WorkflowUpdateFailedException e) when (e.InnerException is ApplicationFailureException
                                                          {
                                                              ErrorType: UserWorkflowBase.StepValidationError
                                                          } failure)
            {
                throw new ApiException("Validation error", failure.Message, StatusCodes.Status400BadRequest);
            }
            catch (WorkflowUpdateFailedException)
            {
                throw NotFound();
            }
            catch (RpcException e) when (e.Code is RpcException.StatusCode.NotFound
                                             or RpcException.StatusCode.FailedPrecondition)
            {
                throw NotFound();
            }

            var description = await handle.DescribeAsync(new WorkflowDescribeOptions { Rpc = rpc });
            return new CompleteUserStepResponse
            {
                WorkflowInstanceId = workflowId,
                Status = description.Status.ToString()
            };
        });
    }

    /// <summary>
    /// Waits briefly until a just started workflow creates its first step; returns <c>null</c> on timeout.
    /// </summary>
    public Task<UserStepDto?> WaitForFirstStepAsync(string workflowId, CancellationToken cancellationToken)
    {
        return client.CallAsync(async () =>
        {
            var deadline = DateTime.UtcNow + FirstStepTimeout;
            do
            {
                var execution = await TryDescribeAsync(workflowId, cancellationToken);
                var steps = execution is null ? [] : await ReadStepsAsync(execution);
                if (steps.Count > 0) return ToDto(steps[0], execution!);
                await Task.Delay(FirstStepPollInterval, cancellationToken);
            } while (DateTime.UtcNow < deadline);

            return null;
        });
    }

    private static string? CreateWorkflowsQuery(string taskQueue, IReadOnlySet<string> workflowNames)
    {
        if (workflowNames.Count == 0) return null;

        var types = string.Join(", ", workflowNames.Select(Quote));
        return $"TaskQueue = {Quote(taskQueue)} AND WorkflowType IN ({types})";
    }

    private static string Quote(string value) => $"'{value.Replace("'", "\\'")}'";

    private static string FormatTime(DateTimeOffset value) =>
        value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

    private async Task<WorkflowExecution?> TryDescribeAsync(string workflowId, CancellationToken cancellationToken)
    {
        try
        {
            return await client.GetWorkflowHandle(workflowId).DescribeAsync(
                new WorkflowDescribeOptions { Rpc = new RpcOptions { CancellationToken = cancellationToken } });
        }
        catch (RpcException e) when (e.Code is RpcException.StatusCode.NotFound
                                         or RpcException.StatusCode.InvalidArgument)
        {
            return null;
        }
    }

    private static async Task<List<UserStep>> ReadStepsAsync(WorkflowExecution execution)
    {
        return execution.Memo.TryGetValue(UserWorkflowBase.StepsMemo, out var memo)
            ? await memo.ToValueAsync<List<UserStep>>()
            : [];
    }

    private UserStepDto ToDto(UserStep step, WorkflowExecution execution)
    {
        var workflowId = execution.Id;
        return new UserStepDto
        {
            Id = step.Id,
            WorkflowInstanceId = workflowId,
            Route = step.Route,
            Url =
                $"{settings.Application.BaseUrl.TrimEnd('/')}/{step.Route}/{Uri.EscapeDataString(workflowId)}/{step.Id}",
            Title = step.Title,
            Description = step.Description,
            Data = step.Data?.GetRawText(),
            CreatedAt = ToUtc(step.CreatedAt),
            Status = step.CompletedAt is not null ? UserStepStatus.Completed
                : execution.Status == WorkflowExecutionStatus.Running ? UserStepStatus.Pending
                : UserStepStatus.Cancelled,
            WorkflowStatus = execution.Status.ToString(),
            CompletedAt = step.CompletedAt is { } completedAt ? ToUtc(completedAt) : null,
            CompletedBy = step.CompletedBy,
            Comment = step.Comment,
            Result = step.Result?.GetRawText()
        };
    }

    private static DateTimeOffset ToUtc(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static JsonElement? ParseResult(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<JsonElement>(json);
        }
        catch (JsonException)
        {
            throw new ApiException("Validation error", "Result is not valid JSON", StatusCodes.Status400BadRequest);
        }
    }

    private static ApiException NotFound() =>
        new("Not found", "Step not found or already completed", StatusCodes.Status404NotFound);

    private static ApiException StepNotFound() =>
        new("Not found", "Step not found", StatusCodes.Status404NotFound);
}
