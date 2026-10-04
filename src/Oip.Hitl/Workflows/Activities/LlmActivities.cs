using Oip.Base.Exceptions;
using Oip.Hitl.Base.Workflows;
using Oip.Hitl.Services;
using Temporalio.Activities;
using Temporalio.Exceptions;

namespace Oip.Hitl.Workflows.Activities;

/// <summary>
/// Temporal activities that call LLM providers.
/// </summary>
public class LlmActivities(LlmProviderService providerService, AgentEventStream eventStream)
{
    /// <summary>
    /// The worker sends heartbeats at most every <see cref="TemporalWorkerService.MaxHeartbeatThrottleInterval"/>,
    /// so a cancellation reaches a turn within about a second.
    /// </summary>
    private static readonly TimeSpan HeartbeatInterval = TemporalWorkerService.MaxHeartbeatThrottleInterval;

    /// <summary>
    /// Sends the request to the provider. A wrong configuration or a request the provider rejects fails without
    /// retries; network errors and provider failures are retried by Temporal.
    /// </summary>
    [Activity]
    public Task<LlmResponse> RequestAsync(LlmRequest request)
    {
        return CallProviderAsync(() =>
            providerService.CompleteChatAsync(request, ActivityExecutionContext.Current.CancellationToken));
    }

    /// <summary>
    /// Runs a turn of an agent and publishes the text deltas of the answer to
    /// <see cref="AgentTurnRequest.StreamKey"/>. The start of each attempt is published too, so the reader can tell
    /// the deltas of a retried attempt apart. Heartbeats are sent while the model answers, so a cancelled workflow
    /// stops the generation and a lost worker is noticed before the activity times out. Failures are handled as in
    /// <see cref="RequestAsync"/>.
    /// </summary>
    [Activity]
    public async Task<AgentTurnResult> ChatTurnAsync(AgentTurnRequest request)
    {
        var context = ActivityExecutionContext.Current;
        var key = request.StreamKey;
        if (key is not null)
            await eventStream.PublishStartAsync(key, context.Info.Attempt);

        using var heartbeatStop = new CancellationTokenSource();
        var heartbeat = HeartbeatAsync(context, heartbeatStop.Token);
        try
        {
            return await CallProviderAsync(() => providerService.StreamChatAsync(request,
                delta => key is null ? Task.CompletedTask : eventStream.PublishDeltaAsync(key, delta),
                context.CancellationToken));
        }
        finally
        {
            await heartbeatStop.CancelAsync();
            await heartbeat;
        }
    }

    private static async Task<T> CallProviderAsync<T>(Func<Task<T>> call)
    {
        try
        {
            return await call();
        }
        catch (ApiException e)
        {
            throw new ApplicationFailureException(e.Message, e, e.Title, nonRetryable: true);
        }
        catch (LlmProviderException e) when (!e.Retryable)
        {
            throw new ApplicationFailureException(e.Message, e, nameof(LlmProviderException), nonRetryable: true);
        }
    }

    private static async Task HeartbeatAsync(ActivityExecutionContext context, CancellationToken stop)
    {
        try
        {
            while (true)
            {
                context.Heartbeat();
                await Task.Delay(HeartbeatInterval, stop);
            }
        }
        catch (OperationCanceledException)
        {
            // The turn is finished.
        }
    }
}
