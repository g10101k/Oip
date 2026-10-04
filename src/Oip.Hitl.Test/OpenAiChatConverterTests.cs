using System.Text.Json;
using Oip.Base.Exceptions;
using Oip.Hitl.Controllers.Api;
using Oip.Hitl.Services;

namespace Oip.Hitl.Test;

public class OpenAiChatConverterTests
{
    [Test]
    public void ToAgentMessages_ConvertsRolesAndTextContent()
    {
        var messages = OpenAiChatConverter.ToAgentMessages([
            Message("developer", "\"Be brief\""),
            Message("user", "\"Hi\""),
            Message("assistant", "null"),
            Message("user", """[{"type":"text","text":"first"},{"type":"text","text":"second"}]""")
        ]);

        Assert.That(messages, Is.EqualTo(new[]
        {
            new AgentMessage("system", "Be brief"),
            new AgentMessage("user", "Hi"),
            new AgentMessage("assistant", ""),
            new AgentMessage("user", "first\nsecond")
        }));
    }

    [Test]
    public void ToAgentMessages_RejectsUnsupportedMessages()
    {
        Assert.Multiple(() =>
        {
            AssertBadRequest(() => OpenAiChatConverter.ToAgentMessages([]));
            AssertBadRequest(() => OpenAiChatConverter.ToAgentMessages([Message("tool", "\"42\"")]));
            AssertBadRequest(() => OpenAiChatConverter.ToAgentMessages([
                Message("user", """[{"type":"image_url","image_url":{"url":"data:image/png;base64,AAAA"}}]""")
            ]));
        });
    }

    [Test]
    public void ToSettings_PassesOnlyProviderParameters()
    {
        var parameters = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            """{"temperature":0.2,"max_tokens":100,"top_p":null,"user":"alice","stream_options":{"include_usage":true}}""")!;

        var settings = OpenAiChatConverter.ToSettings(parameters);

        Assert.That(settings.Keys, Is.EquivalentTo(new[] { "temperature", "max_tokens" }));
        Assert.That(settings["temperature"].GetDouble(), Is.EqualTo(0.2));
    }

    private static ChatCompletionMessage Message(string role, string contentJson) => new()
    {
        Role = role,
        Content = JsonDocument.Parse(contentJson).RootElement.Clone()
    };

    private static void AssertBadRequest(TestDelegate action)
    {
        var exception = Assert.Throws<ApiException>(action);
        Assert.That(exception!.StatusCode, Is.EqualTo(400));
    }
}
