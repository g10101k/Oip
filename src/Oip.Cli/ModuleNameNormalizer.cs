using System.Text;

namespace Oip.Cli;

public static class ModuleNameNormalizer
{
    public static ModuleName Normalize(string value)
    {
        var words = SplitWords(value);
        if (words.Count == 0)
        {
            throw new CliException("Module name must contain letters or digits.");
        }

        if (words[^1].Equals("module", StringComparison.OrdinalIgnoreCase))
        {
            words.RemoveAt(words.Count - 1);
        }

        if (words.Count == 0)
        {
            throw new CliException("Module name must include a name before the word Module.");
        }

        words[^1] = Singularize(words[^1]);
        var pascal = string.Concat(words.Select(ToPascalCase));
        var kebab = string.Join('-', words.Select(w => w.ToLowerInvariant()));

        return new ModuleName(
            pascal,
            pascal,
            $"{pascal}ModuleController",
            $"{pascal}ModuleSettings",
            $"{pascal}ModuleComponent",
            kebab);
    }

    private static List<string> SplitWords(string value)
    {
        var words = new List<string>();
        var current = new StringBuilder();

        foreach (var c in value.Trim())
        {
            if (char.IsLetterOrDigit(c))
            {
                if (current.Length > 0 &&
                    char.IsUpper(c) &&
                    char.IsLower(current[^1]))
                {
                    words.Add(current.ToString());
                    current.Clear();
                }

                current.Append(c);
                continue;
            }

            if (current.Length > 0)
            {
                words.Add(current.ToString());
                current.Clear();
            }
        }

        if (current.Length > 0)
        {
            words.Add(current.ToString());
        }

        return words;
    }

    private static string ToPascalCase(string value)
    {
        var lower = value.ToLowerInvariant();
        return char.ToUpperInvariant(lower[0]) + lower[1..];
    }

    private static string Singularize(string value)
    {
        if (value.Length > 3 && value.EndsWith('s') && !value.EndsWith("ss", StringComparison.OrdinalIgnoreCase))
        {
            return value[..^1];
        }

        return value;
    }
}
