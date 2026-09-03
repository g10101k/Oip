using System.Xml.Linq;

namespace Oip.Cli;

public static class ProjectResolver
{
    public static TargetProject Resolve(
        string? explicitProjectPath,
        string currentDirectory,
        TextReader input,
        TextWriter output,
        string? explicitAngularProject = null)
    {
        var projectPath = explicitProjectPath is not null
            ? Path.GetFullPath(explicitProjectPath, currentDirectory)
            : DiscoverProjectInDirectory(currentDirectory);

        if (!File.Exists(projectPath))
        {
            throw new CliException($"Project file was not found: {projectPath}");
        }

        var projectDirectory = Path.GetDirectoryName(projectPath)!;
        var document = XDocument.Load(projectPath);
        var sdk = document.Root?.Attribute("Sdk")?.Value ?? "";
        if (!sdk.Contains("Microsoft.NET.Sdk.Web", StringComparison.OrdinalIgnoreCase))
        {
            throw new CliException($"Selected project is not an ASP.NET Web project: {projectPath}");
        }

        var spaRootValue = document.Descendants("SpaRoot").FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(spaRootValue))
        {
            throw new CliException($"Selected project does not define SpaRoot: {projectPath}");
        }

        var projectName = Path.GetFileNameWithoutExtension(projectPath);
        var rootNamespace = document.Descendants("RootNamespace").FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(rootNamespace))
        {
            rootNamespace = projectName;
        }

        var spaProxyServerUrl = document.Descendants("SpaProxyServerUrl").FirstOrDefault()?.Value;
        var spaProxyLaunchCommand = document.Descendants("SpaProxyLaunchCommand").FirstOrDefault()?.Value;

        var spaRoot = Path.GetFullPath(NormalizePathSeparators(spaRootValue), projectDirectory);
        var angularProjectPath = explicitAngularProject is not null
            ? ResolveExplicitAngularProject(explicitAngularProject, spaRoot, currentDirectory)
            : ResolveAngularProjectPath(
                projectName,
                spaRoot,
                spaProxyServerUrl,
                spaProxyLaunchCommand,
                currentDirectory,
                input,
                output);

        return new TargetProject(
            projectPath,
            projectDirectory,
            projectName,
            rootNamespace,
            spaRoot,
            angularProjectPath);
    }

    private static string DiscoverProjectInDirectory(string currentDirectory)
    {
        var projects = Directory.GetFiles(currentDirectory, "*.csproj", SearchOption.TopDirectoryOnly);
        return projects.Length switch
        {
            0 => throw new CliException("No .csproj file was found in the current directory. Use --project path/to/App.csproj."),
            1 => projects[0],
            _ => throw new CliException("Multiple .csproj files were found in the current directory. Use --project path/to/App.csproj.")
        };
    }

    private static string ResolveExplicitAngularProject(
        string explicitAngularProject,
        string spaRoot,
        string currentDirectory)
    {
        var byName = AngularProjectDiscovery.TryResolveByName(spaRoot, explicitAngularProject);
        if (byName is not null)
        {
            return byName;
        }

        var path = Path.GetFullPath(NormalizePathSeparators(explicitAngularProject), currentDirectory);
        if (Directory.Exists(path))
        {
            return path;
        }

        throw new CliException(
            $"Angular project was not found in {spaRoot} and is not an existing directory: {explicitAngularProject}");
    }

    private static string ResolveAngularProjectPath(
        string projectName,
        string spaRoot,
        string? spaProxyServerUrl,
        string? spaProxyLaunchCommand,
        string currentDirectory,
        TextReader input,
        TextWriter output)
    {
        var discovered = AngularProjectDiscovery.TryResolve(spaRoot, spaProxyServerUrl, spaProxyLaunchCommand);
        if (discovered is not null)
        {
            return discovered;
        }

        var conventionPath = Path.Combine(spaRoot, "projects", projectName.ToLowerInvariant().Replace('.', '-'));
        if (Directory.Exists(conventionPath))
        {
            return conventionPath;
        }

        output.Write("Angular project path: ");
        var value = input.ReadLine();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new CliException("Angular project path is required because no default mapping exists for this backend project.");
        }

        var path = Path.GetFullPath(NormalizePathSeparators(value), currentDirectory);
        if (!Directory.Exists(path))
        {
            throw new CliException($"Angular project path was not found: {path}");
        }

        return path;
    }

    private static string NormalizePathSeparators(string path)
    {
        return path.Replace('\\', Path.DirectorySeparatorChar);
    }
}
