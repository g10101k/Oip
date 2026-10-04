using System.Text;
using System.Text.Json;
using Oip.Base.Exceptions;
using Oip.Hitl.Controllers.Api;

namespace Oip.Hitl.Services;

/// <summary>
/// Converts requests of the OpenAI chat completions API to agent runs.
/// </summary>
public static class OpenAiChatConverter
{
    /// <summary>
    /// Request parameters passed to the provider; the others, e.g. <c>user</c> or <c>stream_options</c>, are
    /// dropped because they concern the gateway or the client.
    /// </summary>
    public static readonly IReadOnlySet<string> PassedParameters = new HashSet<string>
    {
        "temperature", "top_p", "max_tokens", "max_completion_tokens", "presence_penalty", "frequency_penalty",
        "seed", "stop", "reasoning_effort"
    };

    /// <summary>
    /// Converts the chat history. <c>developer</c> messages become <c>system</c> ones; content parts are joined
    /// into one text.
    /// </summary>
    /// <exception cref="ApiException">The history is empty or has a message of an unsupported role or content.</exception>
    public static List<AgentMessage> ToAgentMessages(IReadOnlyList<ChatCompletionMessage> messages)
    {
        if (messages.Count == 0)
            throw Invalid("messages must not be empty");

        return messages.Select((message, index) => new AgentMessage(ToRole(message.Role, index),
            ToText(message.Content, index))).ToList();
    }

    /// <summary>
    /// Returns the <see cref="PassedParameters"/> of the request.
    /// </summary>
    public static Dictionary<string, JsonElement> ToSettings(IReadOnlyDictionary<string, JsonElement> parameters)
    {
        return parameters
            .Where(x => PassedParameters.Contains(x.Key) && x.Value.ValueKind != JsonValueKind.Null)
            .ToDictionary(x => x.Key, x => x.Value.Clone());
    }

    private static string ToRole(string? role, int index) => role switch
    {
        "system" or "developer" => "system",
        "user" or "assistant" => role,
        _ => throw Invalid($"messages[{index}]: role '{role}' is not supported")
    };

    private static string ToText(JsonElement content, int index)
    {
        switch (content.ValueKind)
        {
            case JsonValueKind.String:
                return content.GetString()!;
            case JsonValueKind.Null or JsonValueKind.Undefined:
                return "";
            case JsonValueKind.Array:
                var text = new StringBuilder();
                foreach (var part in content.EnumerateArray())
                {
                    var type = part.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : null;
                    if (type != "text" || !part.TryGetProperty("text", out var partText))
                        throw Invalid($"messages[{index}]: content of type '{type}' is not supported, only text");
                    if (text.Length > 0) text.Append('\n');
                    text.Append(partText.GetString());
                }

                return text.ToString();
            default:
                throw Invalid($"messages[{index}]: content must be a string or an array of content parts");
        }
    }

    private static ApiException Invalid(string message) =>
        new("Invalid request", message, StatusCodes.Status400BadRequest);
}
