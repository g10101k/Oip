namespace Oip.Cli.Test;

public class AngularProjectDiscoveryTests
{
    private string _temporaryDirectory = null!;
    private string _spaRoot = null!;

    [SetUp]
    public void SetUp()
    {
        _temporaryDirectory = Path.Combine(Path.GetTempPath(), "oip-cli-tests", Guid.NewGuid().ToString("N"));
        _spaRoot = Path.Combine(_temporaryDirectory, "Oip.WebClient");
        Directory.CreateDirectory(_spaRoot);
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
    public void TryResolve_UsesLaunchCommandScript()
    {
        CreateSharedWorkspace();

        var result = AngularProjectDiscovery.TryResolve(_spaRoot, "https://localhost:50002", "npm run start");

        Assert.That(result, Is.EqualTo(Path.Combine(_spaRoot, "projects", "oip")));
    }

    [Test]
    public void TryResolve_UsesLaunchCommandScriptForSecondaryProject()
    {
        CreateSharedWorkspace();

        var result = AngularProjectDiscovery.TryResolve(_spaRoot, "https://localhost:50003", "npm run start:rtds");

        Assert.That(result, Is.EqualTo(Path.Combine(_spaRoot, "projects", "oip-rtds")));
    }

    [Test]
    public void TryResolve_UsesServerUrlPortWhenLaunchCommandIsMissing()
    {
        CreateSharedWorkspace();

        var result = AngularProjectDiscovery.TryResolve(_spaRoot, "https://localhost:50005", null);

        Assert.That(result, Is.EqualTo(Path.Combine(_spaRoot, "projects", "oip-users")));
    }

    [Test]
    public void TryResolve_UsesSingleProjectWorkspace()
    {
        CreateAngularJson("""
            {
              "projects": {
                "oip": {
                  "projectType": "application",
                  "root": "projects/oip"
                }
              }
            }
            """);
        Directory.CreateDirectory(Path.Combine(_spaRoot, "projects", "oip"));

        var result = AngularProjectDiscovery.TryResolve(_spaRoot, "https://localhost:50008", "npm run start");

        Assert.That(result, Is.EqualTo(Path.Combine(_spaRoot, "projects", "oip")));
    }

    [Test]
    public void TryResolve_ReturnsNullWhenPortIsAmbiguous()
    {
        CreateAngularJson("""
            {
              "projects": {
                "first": {
                  "root": "projects/first",
                  "architect": { "serve": { "options": { "port": 50002 } } }
                },
                "second": {
                  "root": "projects/second",
                  "architect": { "serve": { "options": { "port": 50002 } } }
                }
              }
            }
            """);
        Directory.CreateDirectory(Path.Combine(_spaRoot, "projects", "first"));
        Directory.CreateDirectory(Path.Combine(_spaRoot, "projects", "second"));

        var result = AngularProjectDiscovery.TryResolve(_spaRoot, "https://localhost:50002", null);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void TryResolve_ReturnsNullWhenWorkspaceIsMissing()
    {
        var result = AngularProjectDiscovery.TryResolve(_spaRoot, "https://localhost:50002", "npm run start");

        Assert.That(result, Is.Null);
    }

    [Test]
    public void TryResolve_ReturnsNullWhenProjectDirectoryDoesNotExist()
    {
        CreateSharedWorkspace(createDirectories: false);

        var result = AngularProjectDiscovery.TryResolve(_spaRoot, "https://localhost:50002", "npm run start");

        Assert.That(result, Is.Null);
    }

    private void CreateSharedWorkspace(bool createDirectories = true)
    {
        CreateAngularJson("""
            {
              "projects": {
                "oip": {
                  "projectType": "application",
                  "root": "projects/oip",
                  "architect": { "serve": { "options": { "port": 50002 } } }
                },
                "oip-rtds": {
                  "projectType": "application",
                  "root": "projects/oip-rtds",
                  "architect": { "serve": { "options": { "port": 50003 } } }
                },
                "oip-users": {
                  "projectType": "application",
                  "root": "projects/oip-users",
                  "architect": { "serve": { "options": { "port": 50005 } } }
                },
                "oip-common": {
                  "projectType": "library",
                  "root": "projects/oip-common"
                }
              }
            }
            """);

        File.WriteAllText(
            Path.Combine(_spaRoot, "package.json"),
            """
            {
              "name": "oip",
              "scripts": {
                "ng": "ng",
                "start": "run-script-os",
                "start:windows": "ng serve oip --port 50002 --ssl",
                "start:default": "ng serve oip --port 50002 --ssl",
                "start:rtds": "run-script-os",
                "start:rtds:windows": "ng serve oip-rtds --port 50003 --ssl",
                "start:rtds:default": "ng serve oip-rtds --port 50003 --ssl",
                "build:oip": "ng build oip"
              }
            }
            """);

        if (!createDirectories)
        {
            return;
        }

        foreach (var name in new[] { "oip", "oip-rtds", "oip-users", "oip-common" })
        {
            Directory.CreateDirectory(Path.Combine(_spaRoot, "projects", name));
        }
    }

    private void CreateAngularJson(string content)
    {
        File.WriteAllText(Path.Combine(_spaRoot, "angular.json"), content);
    }
}
