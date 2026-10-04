using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Oip.Base.Exceptions;
using Oip.Hitl.Base.Workflows;
using Oip.Hitl.Controllers.Api;
using Oip.Hitl.Data.Contexts;
using Oip.Hitl.Services;
using Oip.Hitl.Workflows.Activities;

namespace Oip.Hitl.Test;

public class AgentServiceTests
{
    private LlmContext _context = null!;
    private AgentService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<LlmContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LlmContext(options);
        var catalog = new AgentToolCatalog(new OipWorkflowOptions().AddActivities<DemoToolActivities>());
        _service = new AgentService(_context, catalog, NullLogger<AgentService>.Instance);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task Snapshot_HasEnabledSkillsOfEnabledAgent()
    {
        var time = await CreateSkillAsync("datetime", "get_current_time");
        var math = await CreateSkillAsync("calculator", "evaluate_expression");
        await _service.UpdateSkillAsync(math.Id, Skill("calculator", "evaluate_expression") with { IsEnabled = false },
            CancellationToken.None);
        await _service.CreateAgentAsync(new SaveAgentRequest("assistant", "Assistant", SystemPrompt: "Be brief.",
            SkillIds: [time.Id, math.Id]), CancellationToken.None);
        await _service.CreateAgentAsync(new SaveAgentRequest("hidden", "Hidden", IsEnabled: false),
            CancellationToken.None);

        var snapshot = await _service.GetSnapshotAsync("assistant", CancellationToken.None);

        Assert.That(snapshot, Is.Not.Null);
        Assert.That(snapshot!.SystemPrompt, Is.EqualTo("Be brief."));
        Assert.That(snapshot.Skills, Is.EqualTo(new[] { new AgentSkillSummary("datetime", "datetime skill") }));
        Assert.That(await _service.GetSnapshotAsync("hidden", CancellationToken.None), Is.Null);
    }

    [Test]
    public async Task LoadSkill_ReturnsInstructionsAndToolsOnlyForSkillOfAgent()
    {
        var time = await CreateSkillAsync("datetime", "get_current_time");
        await CreateSkillAsync("calculator", "evaluate_expression");
        var agent = await _service.CreateAgentAsync(new SaveAgentRequest("assistant", "Assistant",
            SkillIds: [time.Id]), CancellationToken.None);

        var skill = await _service.LoadSkillAsync(new LoadSkillRequest(agent.Id, "datetime"), CancellationToken.None);

        Assert.That(skill.Instructions, Is.EqualTo("Do datetime things."));
        Assert.That(skill.Tools.Select(x => x.Name), Is.EqualTo(new[] { "get_current_time" }));
        var exception = Assert.ThrowsAsync<ApiException>(() =>
            _service.LoadSkillAsync(new LoadSkillRequest(agent.Id, "calculator"), CancellationToken.None));
        Assert.That(exception!.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task Save_ValidatesCodesToolsAndSkills()
    {
        await CreateSkillAsync("datetime", "get_current_time");

        Assert.Multiple(() =>
        {
            AssertBadRequest(() => _service.CreateSkillAsync(Skill("datetime", "get_current_time"),
                CancellationToken.None));
            AssertBadRequest(() => _service.CreateSkillAsync(Skill("Bad Code", "get_current_time"),
                CancellationToken.None));
            AssertBadRequest(() => _service.CreateSkillAsync(Skill("weather", "get_weather"),
                CancellationToken.None));
            AssertBadRequest(() => _service.CreateAgentAsync(new SaveAgentRequest("assistant", "Assistant",
                SkillIds: [42]), CancellationToken.None));
        });
    }

    [Test]
    public async Task UpdateAgent_ReplacesSkills()
    {
        var time = await CreateSkillAsync("datetime", "get_current_time");
        var math = await CreateSkillAsync("calculator", "evaluate_expression");
        var agent = await _service.CreateAgentAsync(new SaveAgentRequest("assistant", "Assistant",
            SkillIds: [time.Id]), CancellationToken.None);

        var updated = await _service.UpdateAgentAsync(agent.Id, new SaveAgentRequest("assistant", "Assistant",
            SkillIds: [time.Id, math.Id]), CancellationToken.None);
        var reduced = await _service.UpdateAgentAsync(agent.Id, new SaveAgentRequest("assistant", "Assistant",
            SkillIds: [math.Id]), CancellationToken.None);

        Assert.That(updated.SkillIds, Is.EquivalentTo(new[] { time.Id, math.Id }));
        Assert.That(reduced.SkillIds, Is.EqualTo(new[] { math.Id }));
    }

    private Task<SkillDto> CreateSkillAsync(string code, string tool) =>
        _service.CreateSkillAsync(Skill(code, tool), CancellationToken.None);

    private static SaveSkillRequest Skill(string code, string tool) =>
        new(code, $"{code} skill", $"Do {code} things.", Tools: [tool]);

    private static void AssertBadRequest(AsyncTestDelegate action)
    {
        var exception = Assert.ThrowsAsync<ApiException>(action);
        Assert.That(exception!.StatusCode, Is.EqualTo(400));
    }
}
