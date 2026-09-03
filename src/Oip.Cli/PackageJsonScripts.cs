using System.Text.Json;

namespace Oip.Cli;

public static class PackageJsonScripts
{
    public static IReadOnlyDictionary<string, string>? TryRead(string spaRoot)
    {
        var path = Path.Combine(spaRoot, "package.json");
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
            if (!document.RootElement.TryGetProperty("scripts", out var scripts)
                || scripts.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var script in scripts.EnumerateObject())
            {
                if (script.Value.ValueKind == JsonValueKind.String)
                {
                    result[script.Name] = script.Value.GetString() ?? "";
                }
            }

            return result;
        }
    }
}
