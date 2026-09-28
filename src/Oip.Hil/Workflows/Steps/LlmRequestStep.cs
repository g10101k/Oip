using System.Text.Json;
using Oip.Hil.Base.Workflows;
using Oip.Hil.Services;
using Oip.Hil.Workflows.Activities;
using Temporalio.Common;
using Temporalio.Workflows;

namespace Oip.Hil.Workflows.Steps;

/// <summary>
/// Request to an LLM provider shown on the <c>workflow-llm-request</c> page. When <see cref="Outcomes"/> are set,
/// the model selects one of them through a tool call and it is returned in <see cref="LlmResponse.Outcome"/>.
/// </summary>
public class LlmRequestStep : AutomatedStepDefinition<LlmResponse>
{
    /// <inheritdoc />
    public override string Route => "workflow-llm-request";

    /// <summary>
    /// User message.
    /// </summary>
    public required string Prompt { get; init; }

    /// <summary>
    /// System message.
    /// </summary>
    public string? SystemPrompt { get; init; }

    /// <summary>
    /// Outcomes the model selects from.
    /// </summary>
    public IReadOnlyList<string> Outcomes
    {
        get;
        init => field = value.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
    } = [];

    /// <summary>
    /// Provider to use; <c>null</c> for the default provider.
    /// </summary>
    public int? ProviderId { get; init; }

    /// <summary>
    /// Model to use; <c>null</c> for the model of the provider.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Request parameters sent to the chat completions API as is, e.g. <c>temperature</c>, <c>top_p</c>,
    /// <c>max_tokens</c>, <c>reasoning_effort</c> or a provider-specific one. Values are serialized to JSON.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Settings { get; init; } = new Dictionary<string, object?>();

    /// <inheritdoc />
    public override object Data => new { Prompt, SystemPrompt, Outcomes, ProviderId, Model, Settings };

    /// <inheritdoc />
    public override Task<LlmResponse> RunAsync(AutomatedStepContext context)
    {
        var settings = Settings.ToDictionary(x => x.Key, x => JsonSerializer.SerializeToElement(x.Value));
        var request = new LlmRequest(Prompt, SystemPrompt, Outcomes, ProviderId, Model, settings);
        return Workflow.ExecuteActivityAsync((LlmActivities activities) => activities.RequestAsync(request),
            new ActivityOptions
            {
                StartToCloseTimeout = TimeSpan.FromMinutes(3),
                RetryPolicy = new RetryPolicy { MaximumAttempts = 3 }
            });
    }

    /// <inheritdoc />
    public override string GetPerformer(LlmResponse result) => $"{result.Provider} · {result.Model}";

    /// <inheritdoc />
    public override IReadOnlyList<WorkflowAttachment> GetAttachments(LlmResponse result) => result.Attachments ?? [];
}
