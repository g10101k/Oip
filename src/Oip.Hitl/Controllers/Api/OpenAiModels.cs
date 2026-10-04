using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Oip.Hitl.Controllers.Api;

/// <summary>
/// Serialization of the responses of the OpenAI-compatible API: snake_case names, no <c>null</c> properties.
/// </summary>
public static class OpenAiJson
{
    /// <summary>
    /// Serializer options of the responses.
    /// </summary>
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        // The responses are not embedded in HTML, so non-ASCII text, e.g. Cyrillic, is written as is.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}

/// <summary>
/// Request of the OpenAI chat completions API.
/// </summary>
public class ChatCompletionRequest
{
    /// <summary>
    /// Model id from <c>GET /v1/models</c>.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Chat history.
    /// </summary>
    public List<ChatCompletionMessage> Messages { get; set; } = [];

    /// <summary>
    /// Whether the answer is streamed as server-sent events.
    /// </summary>
    public bool Stream { get; set; }

    /// <summary>
    /// Not in the OpenAI API: whether the statuses and the user steps of the run are streamed as
    /// <see cref="ChatCompletionChunk.Oip"/> events instead of text, for the Open WebUI pipe of OIP.
    /// </summary>
    [JsonPropertyName("oip_events")]
    public bool OipEvents { get; set; }

    /// <summary>
    /// Other parameters of the request, e.g. <c>temperature</c> or <c>max_tokens</c>.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Parameters { get; set; } = [];
}

/// <summary>
/// Message of <see cref="ChatCompletionRequest"/>.
/// </summary>
public class ChatCompletionMessage
{
    /// <summary>
    /// Author of the message: <c>system</c>, <c>developer</c>, <c>user</c> or <c>assistant</c>.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Text of the message, or an array of content parts.
    /// </summary>
    public JsonElement Content { get; set; }
}

/// <summary>
/// Model of <c>GET /v1/models</c>.
/// </summary>
/// <param name="Id">Model id sent in <see cref="ChatCompletionRequest.Model"/>.</param>
/// <param name="Created">Creation time, Unix seconds.</param>
/// <param name="OwnedBy">Owner of the model.</param>
/// <param name="Name">Display name; not in the OpenAI API, but shown by chat UIs such as Open WebUI.</param>
public record OpenAiModel(string Id, long Created, string OwnedBy, string? Name = null)
{
    /// <summary>
    /// Object type.
    /// </summary>
    public string Object => "model";
}

/// <summary>
/// Response of <c>GET /v1/models</c>.
/// </summary>
/// <param name="Data">Available models.</param>
public record OpenAiModelList(IReadOnlyList<OpenAiModel> Data)
{
    /// <summary>
    /// Object type.
    /// </summary>
    public string Object => "list";
}

/// <summary>
/// Response of the chat completions API without streaming.
/// </summary>
/// <param name="Id">Completion id.</param>
/// <param name="Created">Creation time, Unix seconds.</param>
/// <param name="Model">Model id of the request.</param>
/// <param name="Choices">The answer.</param>
/// <param name="Usage">Token usage, when reported by the provider.</param>
public record ChatCompletion(
    string Id,
    long Created,
    string Model,
    IReadOnlyList<ChatCompletionChoice> Choices,
    ChatCompletionUsage? Usage)
{
    /// <summary>
    /// Object type.
    /// </summary>
    public string Object => "chat.completion";
}

/// <summary>
/// Answer of <see cref="ChatCompletion"/>.
/// </summary>
/// <param name="Index">Index of the answer.</param>
/// <param name="Message">The answer.</param>
/// <param name="FinishReason">Why the model stopped.</param>
public record ChatCompletionChoice(int Index, ChatCompletionResponseMessage Message, string? FinishReason);

/// <summary>
/// Message of <see cref="ChatCompletionChoice"/>.
/// </summary>
/// <param name="Role">Author of the message.</param>
/// <param name="Content">Text of the message.</param>
public record ChatCompletionResponseMessage(string Role, string Content);

/// <summary>
/// Server-sent event of a streamed answer.
/// </summary>
/// <param name="Id">Completion id, the same in all chunks.</param>
/// <param name="Created">Creation time, Unix seconds.</param>
/// <param name="Model">Model id of the request.</param>
/// <param name="Choices">Delta of the answer.</param>
/// <param name="Usage">Token usage, sent in the last chunk when reported by the provider.</param>
/// <param name="Oip">Not in the OpenAI API: event of the run, sent with no choices when
/// <see cref="ChatCompletionRequest.OipEvents"/> is set.</param>
public record ChatCompletionChunk(
    string Id,
    long Created,
    string Model,
    IReadOnlyList<ChatCompletionChunkChoice> Choices,
    ChatCompletionUsage? Usage = null,
    AgentRunEvent? Oip = null)
{
    /// <summary>
    /// Object type.
    /// </summary>
    public string Object => "chat.completion.chunk";
}

/// <summary>
/// Delta of <see cref="ChatCompletionChunk"/>.
/// </summary>
/// <param name="Index">Index of the answer.</param>
/// <param name="Delta">Delta of the message.</param>
/// <param name="FinishReason">Why the model stopped, in the last chunk.</param>
public record ChatCompletionChunkChoice(int Index, ChatCompletionDelta Delta, string? FinishReason = null);

/// <summary>
/// Delta of a streamed message.
/// </summary>
/// <param name="Role">Author of the message, in the first chunk.</param>
/// <param name="Content">Text delta.</param>
/// <param name="ReasoningContent">Reasoning delta, shown by chat UIs apart from the answer.</param>
public record ChatCompletionDelta(string? Role = null, string? Content = null, string? ReasoningContent = null);

/// <summary>
/// Event of an agent run streamed to the Open WebUI pipe of OIP.
/// </summary>
/// <param name="Type"><c>status</c> for progress, e.g. a tool call, or <c>user_step</c> when the run waits for the
/// user.</param>
/// <param name="Text">Text of a status.</param>
/// <param name="RunId">Run of a user step, to complete it with.</param>
/// <param name="StepId">The user step.</param>
/// <param name="Kind"><c>question</c> or <c>approval</c> of a tool call.</param>
/// <param name="Title">Title of the step.</param>
/// <param name="Description">The question, or the tool call to allow.</param>
/// <param name="Outcomes">Answers to choose from; empty for a free answer.</param>
/// <param name="Url">Page of the step in OIP, where it can be completed too.</param>
public record AgentRunEvent(
    string Type,
    string? Text = null,
    string? RunId = null,
    string? StepId = null,
    string? Kind = null,
    string? Title = null,
    string? Description = null,
    IReadOnlyList<string>? Outcomes = null,
    string? Url = null);

/// <summary>
/// Answer of the user to a step of an agent run.
/// </summary>
/// <param name="Result">The answer: one of the outcomes of the step, a free answer, or empty to decline.</param>
/// <param name="Comment">Optional comment, e.g. why a tool call is denied.</param>
public record CompleteAgentStepRequest(string? Result, string? Comment = null);

/// <summary>
/// Token usage of a completion.
/// </summary>
/// <param name="PromptTokens">Tokens in the request.</param>
/// <param name="CompletionTokens">Tokens in the answer.</param>
/// <param name="TotalTokens">Sum of both.</param>
public record ChatCompletionUsage(long PromptTokens, long CompletionTokens, long TotalTokens);

/// <summary>
/// Error response of the OpenAI-compatible API; also sent as the last server-sent event when a stream fails.
/// </summary>
/// <param name="Error">The error.</param>
public record OpenAiErrorResponse(OpenAiError Error);

/// <summary>
/// Error of <see cref="OpenAiErrorResponse"/>.
/// </summary>
/// <param name="Message">Error message shown to the user.</param>
/// <param name="Type">Error type, e.g. <c>invalid_request_error</c>.</param>
/// <param name="Code">Machine-readable code, e.g. <c>model_not_found</c>.</param>
public record OpenAiError(string Message, string Type, string? Code = null);
