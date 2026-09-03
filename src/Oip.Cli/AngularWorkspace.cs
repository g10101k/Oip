using System.Text.Json;

namespace Oip.Cli;

public sealed record AngularProjectInfo(string Name, string Root, int? ServePort);

public sealed record AngularWorkspace(IReadOnlyDictionary<string, AngularProjectInfo> Projects)
{
    public static AngularWorkspace? TryRead(string spaRoot)
    {
        var path = Path.Combine(spaRoot, "angular.json");
        if (!File.Exists(path))
        {
            return null;
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(File.ReadAllText(path), new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });
        }
        catch (JsonException)
        {
            return null;
        }

        using (document)
        {
            if (!document.RootElement.TryGetProperty("projects", out var projects)
                || projects.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var result = new Dictionary<string, AngularProjectInfo>(StringComparer.Ordinal);
            foreach (var project in projects.EnumerateObject())
            {
                if (project.Value.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var root = project.Value.TryGetProperty("root", out var rootElement)
                    ? rootElement.GetString() ?? ""
                    : "";

                result[project.Name] = new AngularProjectInfo(project.Name, root, ReadServePort(project.Value));
            }

            return new AngularWorkspace(result);
        }
    }

    private static int? ReadServePort(JsonElement project)
    {
        if (project.TryGetProperty("architect", out var architect)
            && architect.ValueKind == JsonValueKind.Object
            && architect.TryGetProperty("serve", out var serve)
            && serve.ValueKind == JsonValueKind.Object
            && serve.TryGetProperty("options", out var options)
            && options.ValueKind == JsonValueKind.Object
            && options.TryGetProperty("port", out var port)
            && port.ValueKind == JsonValueKind.Number
            && port.TryGetInt32(out var value))
        {
            return value;
        }

        return null;
    }
}
