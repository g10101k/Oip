using System.Xml.Linq;

namespace Oip.Cli;

public static class ProjectResolver
{
    public static TargetProject Resolve(string? explicitProjectPath, string currentDirectory, TextReader input, TextWriter output)
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

        var spaRoot = Path.GetFullPath(NormalizePathSeparators(spaRootValue), projectDirectory);
        var angularProjectPath = ResolveAngularProjectPath(projectName, spaRoot, currentDirectory, input, output);

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

    private static string ResolveAngularProjectPath(
        string projectName,
        string spaRoot,
        string currentDirectory,
        TextReader input,
        TextWriter output)
    {
        var mappedName = projectName switch
        {
            "Oip" => "oip",
            "Oip.Rtds" => "oip-rtds",
            _ => null
        };

        if (mappedName is not null)
        {
            var mappedPath = Path.Combine(spaRoot, "projects", mappedName);
            if (Directory.Exists(mappedPath))
            {
                return mappedPath;
            }
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
