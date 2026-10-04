using Oip.Base.Exceptions;
using Oip.Hitl.Services;
using Temporalio.Activities;
using Temporalio.Exceptions;

namespace Oip.Hitl.Workflows.Activities;

/// <summary>
/// Temporal activities that call LLM providers.
/// </summary>
public class LlmActivities(LlmProviderService providerService)
{
    /// <summary>
    /// Sends the request to the provider. A wrong configuration or a request the provider rejects fails without
    /// retries; network errors and provider failures are retried by Temporal.
    /// </summary>
    [Activity]
    public async Task<LlmResponse> RequestAsync(LlmRequest request)
    {
        try
        {
            return await providerService.CompleteChatAsync(request, ActivityExecutionContext.Current.CancellationToken);
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
}