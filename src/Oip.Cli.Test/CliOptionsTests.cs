namespace Oip.Cli.Test;

public class CliOptionsTests
{
    [Test]
    public void Parse_ReadsAngularProject()
    {
        var options = CliOptions.Parse(["--name", "Report", "--angular-project", "oip-rtds"]);

        Assert.Multiple(() =>
        {
            Assert.That(options.Name, Is.EqualTo("Report"));
            Assert.That(options.AngularProject, Is.EqualTo("oip-rtds"));
        });
    }

    [Test]
    public void Parse_LeavesAngularProjectEmptyWhenNotSpecified()
    {
        var options = CliOptions.Parse(["--name", "Report"]);

        Assert.That(options.AngularProject, Is.Null);
    }

    [Test]
    public void Parse_ThrowsWhenAngularProjectHasNoValue()
    {
        var ex = Assert.Throws<CliException>(() => CliOptions.Parse(["--angular-project"]));

        Assert.That(ex!.Message, Does.Contain("--angular-project"));
    }
}
