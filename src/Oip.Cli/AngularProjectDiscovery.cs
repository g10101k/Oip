namespace Oip.Cli;

public static class AngularProjectDiscovery
{
    private const string ServeCommand = "ng serve";
    private const int MaxScriptHops = 5;

    public static string? TryResolve(string spaRoot, string? spaProxyServerUrl, string? spaProxyLaunchCommand)
    {
        var workspace = AngularWorkspace.TryRead(spaRoot);
        if (workspace is null || workspace.Projects.Count == 0)
        {
            return null;
        }

        var fromLaunchCommand = ResolveByLaunchCommand(spaRoot, workspace, spaProxyLaunchCommand);
        if (fromLaunchCommand is not null)
        {
            return fromLaunchCommand;
        }

        var fromServerUrl = ResolveByServerUrl(spaRoot, workspace, spaProxyServerUrl);
        if (fromServerUrl is not null)
        {
            return fromServerUrl;
        }

        return workspace.Projects.Count == 1
            ? ResolvePath(spaRoot, workspace.Projects.Values.First())
            : null;
    }

    public static string? TryResolveByName(string spaRoot, string angularProjectName)
    {
        var workspace = AngularWorkspace.TryRead(spaRoot);
        return workspace is not null && workspace.Projects.TryGetValue(angularProjectName, out var project)
            ? ResolvePath(spaRoot, project)
            : null;
    }

    private static string? ResolveByLaunchCommand(string spaRoot, AngularWorkspace workspace, string? spaProxyLaunchCommand)
    {
        if (string.IsNullOrWhiteSpace(spaProxyLaunchCommand))
        {
            return null;
        }

        var scripts = PackageJsonScripts.TryRead(spaRoot);
        if (scripts is null)
        {
            return null;
        }

        var scriptName = ExtractScriptName(spaProxyLaunchCommand);
        if (scriptName is null)
        {
            return null;
        }

        var command = ResolveScriptCommand(scripts, scriptName, 0);
        var projectName = ExtractAngularProjectName(command);
        if (projectName is null || !workspace.Projects.TryGetValue(projectName, out var project))
        {
            return null;
        }

        return ResolvePath(spaRoot, project);
    }

    private static string? ResolveByServerUrl(string spaRoot, AngularWorkspace workspace, string? spaProxyServerUrl)
    {
        var port = ExtractPort(spaProxyServerUrl);
        if (port is null)
        {
            return null;
        }

        var matches = workspace.Projects.Values.Where(x => x.ServePort == port).ToList();
        return matches.Count == 1 ? ResolvePath(spaRoot, matches[0]) : null;
    }

    private static string? ExtractScriptName(string launchCommand)
    {
        var tokens = launchCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 2)
        {
            return null;
        }

        var runner = tokens[0];
        if (runner is not ("npm" or "pnpm" or "yarn"))
        {
            return null;
        }

        if (tokens.Length >= 3 && tokens[1] == "run")
        {
            return tokens[2];
        }

        if (runner == "npm" && tokens[1] == "start")
        {
            return "start";
        }

        return runner == "yarn" ? tokens[1] : null;
    }

    private static string? ResolveScriptCommand(IReadOnlyDictionary<string, string> scripts, string scriptName, int hop)
    {
        if (hop > MaxScriptHops || !scripts.TryGetValue(scriptName, out var command))
        {
            return null;
        }

        if (command.Contains(ServeCommand, StringComparison.Ordinal))
        {
            return command;
        }

        // Scripts such as run-script-os delegate to platform-suffixed siblings; ":default" is the portable one.
        var prefix = scriptName + ":";
        if (scripts.ContainsKey(prefix + "default"))
        {
            return ResolveScriptCommand(scripts, prefix + "default", hop + 1);
        }

        var sibling = scripts
            .Where(x => x.Key.StartsWith(prefix, StringComparison.Ordinal)
                        && x.Value.Contains(ServeCommand, StringComparison.Ordinal))
            .Select(x => x.Value)
            .FirstOrDefault();

        return sibling;
    }

    private static string? ExtractAngularProjectName(string? command)
    {
        if (command is null)
        {
            return null;
        }

        var tokens = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i + 2 < tokens.Length; i++)
        {
            if (tokens[i] != "ng" || tokens[i + 1] != "serve")
            {
                continue;
            }

            var candidate = tokens[i + 2];
            return candidate.StartsWith('-') ? null : candidate;
        }

        return null;
    }

    private static int? ExtractPort(string? spaProxyServerUrl)
    {
        if (string.IsNullOrWhiteSpace(spaProxyServerUrl))
        {
            return null;
        }

        return Uri.TryCreate(spaProxyServerUrl, UriKind.Absolute, out var uri) ? uri.Port : null;
    }

    private static string? ResolvePath(string spaRoot, AngularProjectInfo project)
    {
        var path = string.IsNullOrWhiteSpace(project.Root)
            ? spaRoot
            : Path.Combine(spaRoot, project.Root.Replace('/', Path.DirectorySeparatorChar));

        path = Path.GetFullPath(path);
        return Directory.Exists(path) ? path : null;
    }
}
