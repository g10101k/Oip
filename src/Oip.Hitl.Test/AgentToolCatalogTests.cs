using Microsoft.Extensions.Logging.Abstractions;
using Oip.Hitl.Base.Workflows;
using Oip.Hitl.Services;
using Oip.Hitl.Workflows.Activities;
using Temporalio.Exceptions;

namespace Oip.Hitl.Test;

public class AgentToolCatalogTests
{
    [Test]
    public void Catalog_DescribesToolsOfRegisteredActivities()
    {
        var catalog = new AgentToolCatalog(new OipWorkflowOptions().AddActivities<DemoToolActivities>());

        var tool = catalog.Find("get_current_time");

        Assert.That(catalog.Tools.Select(x => x.Name),
            Is.EqualTo(new[] { "evaluate_expression", "get_current_time", "send_notification" }));
        Assert.That(tool, Is.Not.Null);
        Assert.That(tool!.ActivityName, Is.EqualTo("GetCurrentTime"));
        Assert.That(tool.HasArguments, Is.True);
        Assert.That(tool.TimeoutSeconds, Is.EqualTo(10));
        Assert.That(tool.RequiresApproval, Is.False);
        Assert.That(catalog.Find("send_notification")!.RequiresApproval, Is.True);
        Assert.That(tool.Description, Does.Contain("current date"));
        var property = tool.ParametersSchema.GetProperty("properties").GetProperty("time_zone");
        Assert.That(property.GetProperty("description").GetString(), Does.Contain("IANA"));
    }

    [Test]
    public void EvaluateExpression_ComputesArithmeticOnly()
    {
        var tools = new DemoToolActivities(NullLogger<DemoToolActivities>.Instance);

        Assert.That(tools.EvaluateExpression(new() { Expression = "(2 + 3) * 4.5" }), Is.EqualTo("22.5"));
        Assert.Throws<ApplicationFailureException>(() => tools.EvaluateExpression(new() { Expression = "Len('abc')" }));
    }

    [Test]
    public void GetCurrentTime_RejectsUnknownTimeZone()
    {
        var tools = new DemoToolActivities(NullLogger<DemoToolActivities>.Instance);

        Assert.That(tools.GetCurrentTime(new() { TimeZone = "Europe/Moscow" }), Does.EndWith("(Europe/Moscow)"));
        Assert.Throws<ApplicationFailureException>(() => tools.GetCurrentTime(new() { TimeZone = "Mars/Olympus" }));
    }
}
