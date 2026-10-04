using System.Text.Json;
using Oip.Hitl.Services;
using Oip.Hitl.Workflows;
using Temporalio.Activities;
using Temporalio.Client;
using Temporalio.Exceptions;
using Temporalio.Testing;
using Temporalio.Worker;

namespace Oip.Hitl.Test;

/// <summary>
/// Runs <see cref="AgentWorkflow"/> on the Temporal test server, which is downloaded on the first run.
/// </summary>
public class AgentWorkflowTests
{
    private const string TaskQueue = "agent-workflow-test";

    private static readonly AgentSnapshot Agent = new(1, "assistant", 7, "Be brief.",
        [new AgentSkillSummary("time", "Tells the time.")]);

    private static readonly LoadedSkill TimeSkill = new("time", "Call get_time for the time.",
    [
        new AgentToolDefinition("get_time", "Returns the time.",
            JsonDocument.Parse("""{"type":"object","properties":{"zone":{"type":"string"}}}""").RootElement.Clone(),
            "FakeTool", true, null, 10, 1)
    ]);

    private WorkflowEnvironment _environment = null!;

    [OneTimeSetUp]
    public async Task StartEnvironment() => _environment = await WorkflowEnvironment.StartTimeSkippingAsync();

    [OneTimeTearDown]
    public async Task StopEnvironment() => await _environment.ShutdownAsync();

    [Test]
    public async Task RunAsync_LoadsSkillAndCallsItsTool()
    {
        var activities = new FakeActivities(
            Turn(ToolCall("1", AgentWorkflow.LoadSkillToolName, """{"name":"time"}""")),
            Turn(ToolCall("2", "get_time", """{"zone":"UTC"}""")),
            Turn(content: "It is 12:00."));

        var result = await RunAsync(activities, Input());

        Assert.That(result.Content, Is.EqualTo("It is 12:00."));
        Assert.That(result.PromptTokens, Is.EqualTo(9));
        Assert.That(result.ToolCalls, Is.Null);

        var requests = activities.TurnRequests;
        Assert.That(requests, Has.Count.EqualTo(3));
        Assert.That(requests[0].ProviderId, Is.EqualTo(7));
        Assert.That(requests[0].Messages[0].Role, Is.EqualTo("system"));
        Assert.That(requests[0].Messages[0].Content, Does.StartWith("Be brief.").And.Contain("- time: Tells the time."));
        Assert.That(ToolNames(requests[0]), Is.EqualTo(new[] { AgentWorkflow.LoadSkillToolName }));
        // All skills are loaded, so only their tools are offered.
        Assert.That(ToolNames(requests[1]), Is.EqualTo(new[] { "get_time" }));
        Assert.That(requests[1].Messages[^1].Content, Does.StartWith("Call get_time for the time."));

        Assert.That(activities.ToolArguments.Single().GetProperty("zone").GetString(), Is.EqualTo("UTC"));
        Assert.That(requests[2].Messages[^1], Is.EqualTo(new AgentMessage("tool", "12:00", ToolCallId: "2")));
    }

    [Test]
    public async Task RunAsync_ReportsFailedToolCallToModel()
    {
        var activities = new FakeActivities(
            Turn(ToolCall("1", AgentWorkflow.LoadSkillToolName, """{"name":"time"}""")),
            Turn(ToolCall("2", "get_time", "{}")),
            Turn(ToolCall("3", "not_loaded", "{}")),
            Turn(content: "Sorry.")) { ToolError = "clock is broken" };

        var result = await RunAsync(activities, Input());

        Assert.That(result.Content, Is.EqualTo("Sorry."));
        Assert.That(activities.TurnRequests[2].Messages[^1].Content, Is.EqualTo("Error: clock is broken"));
        Assert.That(activities.TurnRequests[3].Messages[^1].Content, Does.StartWith("Error: tool not_loaded"));
    }

    [Test]
    public async Task RunAsync_OffersNoToolsOnLastTurn()
    {
        var activities = new FakeActivities { Fallback = Turn(ToolCall("1", "not_loaded", "{}"), "Thinking.") };

        var result = await RunAsync(activities, Input());

        Assert.That(activities.TurnRequests, Has.Count.EqualTo(AgentWorkflow.MaxTurns));
        Assert.That(activities.TurnRequests[^1].Tools, Is.Empty);
        Assert.That(result.Content, Does.StartWith("Thinking.\n\nThinking."));
    }

    [Test]
    public void RunAsync_FailsWithCauseWhenProviderRejectsRequest()
    {
        var activities = new FakeActivities { TurnError = "model not found" };

        var exception = Assert.ThrowsAsync<WorkflowFailedException>(() => RunAsync(activities, Input()));

        Assert.That(exception!.InnerException?.InnerException?.Message, Is.EqualTo("model not found"));
        Assert.That(activities.TurnRequests, Has.Count.EqualTo(1), "a rejected request is not retried");
    }

    private static AgentWorkflowInput Input() =>
        new(Agent, [new AgentMessage("user", "What time is it?")], null, null, "alice");

    private static AgentTurnResult Turn(AgentToolCall? call = null, string content = "") =>
        new(content, "Test", "test-model", 3, 1, call is null ? "stop" : "tool_calls", call is null ? null : [call]);

    private static AgentToolCall ToolCall(string id, string name, string arguments) => new(id, name, arguments);

    private static IEnumerable<string> ToolNames(AgentTurnRequest request) => request.Tools!.Select(x => x.Name);

    private async Task<AgentTurnResult> RunAsync(FakeActivities activities, AgentWorkflowInput input)
    {
        using var worker = new TemporalWorker(_environment.Client, new TemporalWorkerOptions(TaskQueue)
            .AddWorkflow<AgentWorkflow>()
            .AddAllActivities(activities));
        return await worker.ExecuteAsync(() => _environment.Client.ExecuteWorkflowAsync(
            (AgentWorkflow workflow) => workflow.RunAsync(input),
            new WorkflowOptions($"agent-{Guid.NewGuid():N}", TaskQueue)));
    }

    /// <summary>
    /// Replaces the activities of the agent, registered under the same names, and the tool of <see cref="TimeSkill"/>.
    /// </summary>
    private sealed class FakeActivities(params AgentTurnResult[] turns)
    {
        private readonly Queue<AgentTurnResult> _turns = new(turns);

        public AgentTurnResult? Fallback { get; init; }

        public string? TurnError { get; init; }

        public string? ToolError { get; init; }

        public List<AgentTurnRequest> TurnRequests { get; } = [];

        public List<JsonElement> ToolArguments { get; } = [];

        [Activity("ChatTurn")]
        public AgentTurnResult ChatTurn(AgentTurnRequest request)
        {
            TurnRequests.Add(request);
            if (TurnError is not null)
                throw new ApplicationFailureException(TurnError, nameof(LlmProviderException), nonRetryable: true);
            return _turns.TryDequeue(out var turn) ? turn : Fallback ?? Turn(content: "Done.");
        }

        [Activity("LoadSkill")]
        public LoadedSkill LoadSkill(LoadSkillRequest request)
        {
            if (request.AgentId != Agent.AgentId || request.Code != TimeSkill.Code)
                throw new ApplicationFailureException($"Skill '{request.Code}' is not available", nonRetryable: true);
            return TimeSkill;
        }

        [Activity("FakeTool")]
        public string FakeTool(JsonElement arguments)
        {
            ToolArguments.Add(arguments);
            if (ToolError is not null)
                throw new ApplicationFailureException(ToolError, nonRetryable: true);
            return "12:00";
        }
    }
}
