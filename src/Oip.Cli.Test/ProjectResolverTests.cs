namespace Oip.Cli.Test;

public class ProjectResolverTests
{
    private string _temporaryDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), "oip-cli-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_temporaryDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_temporaryDirectory))
        {
            Directory.Delete(_temporaryDirectory, true);
        }
    }

    [Test]
    public void Resolve_UsesCurrentDirectoryProject()
    {
        var projectPath = CreateAspNetProject(_temporaryDirectory, "Oip", @"Oip.WebClient\");
        CreateAngularProject("oip");

        var result = ProjectResolver.Resolve(null, _temporaryDirectory, TextReader.Null, TextWriter.Null);

        Assert.That(result.ProjectPath, Is.EqualTo(projectPath));
        Assert.That(result.AngularProjectPath, Is.EqualTo(Path.Combine(_temporaryDirectory, "Oip.WebClient", "projects", "oip")));
    }

    [Test]
    public void Resolve_UsesExplicitProject()
    {
        var projectDirectory = Path.Combine(_temporaryDirectory, "backend");
        Directory.CreateDirectory(projectDirectory);
        var projectPath = CreateAspNetProject(projectDirectory, "Oip", @"..\Oip.WebClient\");
        CreateAngularProject("oip");

        var result = ProjectResolver.Resolve(Path.Combine("backend", "Oip.csproj"), _temporaryDirectory, TextReader.Null, TextWriter.Null);

        Assert.That(result.ProjectPath, Is.EqualTo(projectPath));
    }

    [Test]
    public void Resolve_ThrowsWhenProjectIsMissing()
    {
        var ex = Assert.Throws<CliException>(() =>
            ProjectResolver.Resolve(null, _temporaryDirectory, TextReader.Null, TextWriter.Null));

        Assert.That(ex!.Message, Does.Contain("No .csproj"));
    }

    [Test]
    public void Resolve_ThrowsWhenMultipleProjectsExist()
    {
        CreateAspNetProject(_temporaryDirectory, "Oip", @"Oip.WebClient\");
        CreateAspNetProject(_temporaryDirectory, "Oip.Rtds", @"Oip.WebClient\");

        var ex = Assert.Throws<CliException>(() =>
            ProjectResolver.Resolve(null, _temporaryDirectory, TextReader.Null, TextWriter.Null));

        Assert.That(ex!.Message, Does.Contain("Multiple .csproj"));
    }

    private string CreateAspNetProject(string directory, string name, string spaRoot)
    {
        var projectPath = Path.Combine(directory, $"{name}.csproj");
        File.WriteAllText(
            projectPath,
            $$"""
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <SpaRoot>{{spaRoot}}</SpaRoot>
              </PropertyGroup>
            </Project>
            """);
        return projectPath;
    }

    private void CreateAngularProject(string name)
    {
        Directory.CreateDirectory(Path.Combine(_temporaryDirectory, "Oip.WebClient", "projects", name));
    }
}
