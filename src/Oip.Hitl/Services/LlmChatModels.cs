using System.Text.Json;
using Oip.Hitl.Base.Workflows;

namespace Oip.Hitl.Services;

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
/// Message of a chat with an agent.
/// </summary>
/// <param name="Role">Author of the message: <c>system</c>, <c>user</c> or <c>assistant</c>.</param>
/// <param name="Content">Text of the message.</param>
public record AgentMessage(string Role, string Content);

/// <summary>
/// Turn of an agent: the chat is sent to the model and its answer is streamed.
/// </summary>
/// <param name="ProviderId">Provider to use.</param>
/// <param name="Messages">Chat history; the model answers the last message.</param>
/// <param name="Settings">Request parameters sent to the chat completions API as is, e.g. <c>temperature</c>.</param>
/// <param name="StreamKey">Key of the <see cref="AgentEventStream"/> the text deltas of the answer are published to; <c>null</c> to not stream.</param>
public record AgentTurnRequest(
    int ProviderId,
    IReadOnlyList<AgentMessage> Messages,
    IReadOnlyDictionary<string, JsonElement>? Settings = null,
    string? StreamKey = null);

/// <summary>
/// Answer of the model in an agent turn.
/// </summary>
/// <param name="Content">Text of the answer.</param>
/// <param name="Provider">Name of the provider that answered.</param>
/// <param name="Model">Model that answered.</param>
/// <param name="PromptTokens">Tokens in the request, when reported by the provider.</param>
/// <param name="CompletionTokens">Tokens in the answer, when reported by the provider.</param>
/// <param name="FinishReason">Why the model stopped, e.g. <c>stop</c> or <c>length</c>, when reported by the provider.</param>
public record AgentTurnResult(
    string Content,
    string Provider,
    string Model,
    long? PromptTokens,
    long? CompletionTokens,
    string? FinishReason);

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
