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

    private WorkflowEnvironment _environment = null!;

    [OneTimeSetUp]
    public async Task StartEnvironment() => _environment = await WorkflowEnvironment.StartTimeSkippingAsync();

    [OneTimeTearDown]
    public async Task StopEnvironment() => await _environment.ShutdownAsync();

    [Test]
    public async Task RunAsync_ReturnsAnswerOfTurn()
    {
        var activities = new FakeLlmActivities();
        var input = new AgentWorkflowInput(7, [new AgentMessage("user", "Hi")], null, "oip:agent:test", "alice");

        var result = await RunAsync(activities, input);

        Assert.That(result.Content, Is.EqualTo("Hello"));
        Assert.That(activities.Request, Is.Not.Null);
        Assert.That(activities.Request!.ProviderId, Is.EqualTo(7));
        Assert.That(activities.Request.StreamKey, Is.EqualTo("oip:agent:test"));
        Assert.That(activities.Request.Messages, Is.EqualTo(input.Messages));
    }

    [Test]
    public void RunAsync_FailsWithCauseWhenProviderRejectsRequest()
    {
        var activities = new FakeLlmActivities { Error = "model not found" };
        var input = new AgentWorkflowInput(7, [new AgentMessage("user", "Hi")], null, null, null);

        var exception = Assert.ThrowsAsync<WorkflowFailedException>(() => RunAsync(activities, input));

        Assert.That(exception!.InnerException?.InnerException?.Message, Is.EqualTo("model not found"));
        Assert.That(activities.Attempts, Is.EqualTo(1), "a rejected request is not retried");
    }

    private async Task<AgentTurnResult> RunAsync(FakeLlmActivities activities, AgentWorkflowInput input)
    {
        using var worker = new TemporalWorker(_environment.Client, new TemporalWorkerOptions(TaskQueue)
            .AddWorkflow<AgentWorkflow>()
            .AddAllActivities(activities));
        return await worker.ExecuteAsync(() => _environment.Client.ExecuteWorkflowAsync(
            (AgentWorkflow workflow) => workflow.RunAsync(input),
            new WorkflowOptions($"agent-{Guid.NewGuid():N}", TaskQueue)));
    }

    /// <summary>
    /// Replaces <see cref="Workflows.Activities.LlmActivities.ChatTurnAsync"/>, registered under the same name.
    /// </summary>
    private sealed class FakeLlmActivities
    {
        public string? Error { get; init; }

        public AgentTurnRequest? Request { get; private set; }

        public int Attempts { get; private set; }

        [Activity("ChatTurn")]
        public AgentTurnResult ChatTurn(AgentTurnRequest request)
        {
            Request = request;
            Attempts++;
            if (Error is not null)
                throw new ApplicationFailureException(Error, nameof(LlmProviderException), nonRetryable: true);
            return new AgentTurnResult("Hello", "Test", "test-model", 3, 1, "stop");
        }
    }
}
