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
    public void Resolve_DiscoversAngularProjectFromWorkspace()
    {
        CreateAspNetProject(
            _temporaryDirectory,
            "Oip.Notifications",
            @"Oip.WebClient\",
            "https://localhost:50003",
            "npm run start:rtds");
        CreateAngularProject("oip");
        CreateAngularProject("oip-rtds");
        File.WriteAllText(
            Path.Combine(_temporaryDirectory, "Oip.WebClient", "angular.json"),
            """
            {
              "projects": {
                "oip": {
                  "root": "projects/oip",
                  "architect": { "serve": { "options": { "port": 50002 } } }
                },
                "oip-rtds": {
                  "root": "projects/oip-rtds",
                  "architect": { "serve": { "options": { "port": 50003 } } }
                }
              }
            }
            """);
        File.WriteAllText(
            Path.Combine(_temporaryDirectory, "Oip.WebClient", "package.json"),
            """
            {
              "scripts": {
                "start": "run-script-os",
                "start:default": "ng serve oip --port 50002",
                "start:rtds": "run-script-os",
                "start:rtds:default": "ng serve oip-rtds --port 50003"
              }
            }
            """);

        var result = ProjectResolver.Resolve(null, _temporaryDirectory, TextReader.Null, TextWriter.Null);

        Assert.That(
            result.AngularProjectPath,
            Is.EqualTo(Path.Combine(_temporaryDirectory, "Oip.WebClient", "projects", "oip-rtds")));
    }

    [Test]
    public void Resolve_UsesExplicitAngularProjectName()
    {
        CreateAspNetProject(_temporaryDirectory, "Oip", @"Oip.WebClient\");
        CreateAngularProject("oip");
        CreateAngularProject("oip-rtds");
        File.WriteAllText(
            Path.Combine(_temporaryDirectory, "Oip.WebClient", "angular.json"),
            """
            {
              "projects": {
                "oip": { "root": "projects/oip" },
                "oip-rtds": { "root": "projects/oip-rtds" }
              }
            }
            """);

        var result = ProjectResolver.Resolve(
            null,
            _temporaryDirectory,
            TextReader.Null,
            TextWriter.Null,
            "oip-rtds");

        Assert.That(
            result.AngularProjectPath,
            Is.EqualTo(Path.Combine(_temporaryDirectory, "Oip.WebClient", "projects", "oip-rtds")));
    }

    [Test]
    public void Resolve_UsesExplicitAngularProjectPath()
    {
        CreateAspNetProject(_temporaryDirectory, "Oip", @"Oip.WebClient\");
        CreateAngularProject("custom");

        var result = ProjectResolver.Resolve(
            null,
            _temporaryDirectory,
            TextReader.Null,
            TextWriter.Null,
            Path.Combine("Oip.WebClient", "projects", "custom"));

        Assert.That(
            result.AngularProjectPath,
            Is.EqualTo(Path.Combine(_temporaryDirectory, "Oip.WebClient", "projects", "custom")));
    }

    [Test]
    public void Resolve_ThrowsWhenExplicitAngularProjectIsUnknown()
    {
        CreateAspNetProject(_temporaryDirectory, "Oip", @"Oip.WebClient\");
        CreateAngularProject("oip");

        var ex = Assert.Throws<CliException>(() => ProjectResolver.Resolve(
            null,
            _temporaryDirectory,
            TextReader.Null,
            TextWriter.Null,
            "oip-missing"));

        Assert.That(ex!.Message, Does.Contain("oip-missing"));
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

    private string CreateAspNetProject(
        string directory,
        string name,
        string spaRoot,
        string? spaProxyServerUrl = null,
        string? spaProxyLaunchCommand = null)
    {
        var projectPath = Path.Combine(directory, $"{name}.csproj");
        var spaProxyProperties = spaProxyServerUrl is null && spaProxyLaunchCommand is null
            ? ""
            : $"""

                 <SpaProxyServerUrl>{spaProxyServerUrl}</SpaProxyServerUrl>
                 <SpaProxyLaunchCommand>{spaProxyLaunchCommand}</SpaProxyLaunchCommand>
               """;

        File.WriteAllText(
            projectPath,
            $$"""
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <SpaRoot>{{spaRoot}}</SpaRoot>{{spaProxyProperties}}
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
