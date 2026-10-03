using System.Text.Json;
using Oip.Hil.Base.Workflows;

namespace Oip.Hil.Services;

/// <summary>
/// Chat completion request to an LLM provider.
/// </summary>
/// <param name="Prompt">User message.</param>
/// <param name="SystemPrompt">System message; <c>null</c> to send none.</param>
/// <param name="Outcomes">Outcomes the model selects from through the <c>select_outcome</c> tool call. Empty or <c>null</c> for a plain text answer.</param>
/// <param name="ProviderId">Provider to use; <c>null</c> for the default provider.</param>
/// <param name="Model">Model to use; <c>null</c> for the model of the provider.</param>
/// <param name="Settings">Request parameters sent to the chat completions API as is, e.g. <c>temperature</c> or <c>max_tokens</c>.</param>
/// <param name="UseShell">Whether the model may run shell commands in the folder of the step.</param>
/// <param name="WorkflowId">Workflow of the step; required with <paramref name="UseShell"/>.</param>
/// <param name="StepId">Step whose folder is the working directory of the shell; required with <paramref name="UseShell"/>.</param>
/// <param name="InputFiles">Files of the workflow the model reads through the shell.</param>
public record LlmRequest(
    string Prompt,
    string? SystemPrompt = null,
    IReadOnlyList<string>? Outcomes = null,
    int? ProviderId = null,
    string? Model = null,
    IReadOnlyDictionary<string, JsonElement>? Settings = null,
    bool UseShell = false,
    string? WorkflowId = null,
    string? StepId = null,
    IReadOnlyList<WorkflowAttachment>? InputFiles = null);

/// <summary>
/// Answer of an LLM provider.
/// </summary>
/// <param name="Content">Text of the answer; when an outcome is selected, the reason the model gave for it.</param>
/// <param name="Outcome">Outcome selected through the tool call; <c>null</c> when no outcomes were requested.</param>
/// <param name="Provider">Name of the provider that answered.</param>
/// <param name="Model">Model that answered.</param>
/// <param name="PromptTokens">Tokens in the request, when reported by the provider.</param>
/// <param name="CompletionTokens">Tokens in the answer, when reported by the provider.</param>
/// <param name="ElapsedMs">Round-trip time in milliseconds.</param>
/// <param name="Attachments">Files the model created in the folder of the step through the shell.</param>
public record LlmResponse(
    string? Content,
    string? Outcome,
    string Provider,
    string Model,
    long? PromptTokens,
    long? CompletionTokens,
    long ElapsedMs,
    IReadOnlyList<WorkflowAttachment>? Attachments = null);

/// <summary>
/// Failed call to an LLM provider.
/// </summary>
/// <param name="message">Error message.</param>
/// <param name="retryable">Whether repeating the same request may succeed.</param>
public class LlmProviderException(string message, bool retryable) : Exception(message)
{
    /// <summary>
    /// Whether repeating the same request may succeed.
    /// </summary>
    public bool Retryable { get; } = retryable;
}
