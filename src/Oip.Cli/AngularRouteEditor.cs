using System.Text.RegularExpressions;

namespace Oip.Cli;

public static partial class AngularRouteEditor
{
    public static RouteInsertionResult InsertModuleRoute(string content, ModuleName module)
    {
        var routePath = $"{module.RoutePath}/:id";
        if (content.Contains($"path: '{routePath}'", StringComparison.Ordinal))
        {
            return RouteInsertionResult.Duplicate;
        }

        var match = ChildrenArrayRegex().Match(content);
        if (!match.Success)
        {
            return RouteInsertionResult.ChildrenNotFound;
        }

        var openIndex = match.Index + match.Length - 1;
        var closeIndex = FindMatchingBracket(content, openIndex);
        if (closeIndex < 0)
        {
            return RouteInsertionResult.ChildrenNotFound;
        }

        var newLine = content.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var childrenIndent = GetLineIndent(content, match.Index);
        var itemIndent = GetFirstItemIndent(content, openIndex + 1, closeIndex) ?? childrenIndent + "  ";
        var route = BuildRoute(module, itemIndent, ResolveGuard(content), newLine);

        if (IsEmptyArray(content, openIndex, closeIndex))
        {
            var body = newLine + route + newLine + childrenIndent;
            return RouteInsertionResult.Inserted(content[..(openIndex + 1)] + body + content[closeIndex..]);
        }

        var lastItemEnd = closeIndex - 1;
        while (lastItemEnd > openIndex && char.IsWhiteSpace(content[lastItemEnd]))
        {
            lastItemEnd--;
        }

        var separator = content[lastItemEnd] == ',' ? string.Empty : ",";
        return RouteInsertionResult.Inserted(content.Insert(lastItemEnd + 1, separator + newLine + route));
    }

    private static bool IsEmptyArray(string content, int openIndex, int closeIndex)
    {
        for (var i = openIndex + 1; i < closeIndex; i++)
        {
            if (!char.IsWhiteSpace(content[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static string? ResolveGuard(string content)
    {
        if (content.Contains("oipAuthGuard", StringComparison.Ordinal))
        {
            return "canActivate: [oipAuthGuard]";
        }

        if (content.Contains("AuthGuardService", StringComparison.Ordinal))
        {
            return "canActivate: [() => inject(AuthGuardService).canActivate()]";
        }

        return null;
    }

    private static string GetLineIndent(string content, int index)
    {
        var lineStart = content.LastIndexOf('\n', Math.Max(index - 1, 0)) + 1;
        var end = lineStart;
        while (end < content.Length && (content[end] == ' ' || content[end] == '\t'))
        {
            end++;
        }

        return content[lineStart..end];
    }

    private static string? GetFirstItemIndent(string content, int start, int end)
    {
        for (var i = start; i < end; i++)
        {
            if (char.IsWhiteSpace(content[i]))
            {
                continue;
            }

            var indent = GetLineIndent(content, i);
            return i >= indent.Length && content.LastIndexOf('\n', i) >= 0 ? indent : null;
        }

        return null;
    }

    private static int FindMatchingBracket(string content, int openIndex)
    {
        var depth = 0;
        var quote = '\0';
        for (var i = openIndex; i < content.Length; i++)
        {
            var current = content[i];
            if (quote != '\0')
            {
                if (current == '\\')
                {
                    i++;
                }
                else if (current == quote)
                {
                    quote = '\0';
                }

                continue;
            }

            switch (current)
            {
                case '\'':
                case '"':
                case '`':
                    quote = current;
                    break;
                case '[':
                    depth++;
                    break;
                case ']':
                    depth--;
                    if (depth == 0)
                    {
                        return i;
                    }

                    break;
            }
        }

        return -1;
    }

    private static string BuildRoute(ModuleName module, string indent, string? guard, string newLine)
    {
        var componentImport =
            $"./app/components/{module.ComponentFolder}/{Path.GetFileNameWithoutExtension(module.ComponentFileName)}";

        var lines = new List<string>
        {
            $"{indent}{{",
            $"{indent}  path: '{module.RoutePath}/:id',",
            $"{indent}  loadComponent: () =>",
            $"{indent}    import('{componentImport}').then(",
            $"{indent}      (m) => m.{module.ComponentClassName}",
            guard is null ? $"{indent}    )" : $"{indent}    ),"
        };

        if (guard is not null)
        {
            lines.Add($"{indent}  {guard}");
        }

        lines.Add($"{indent}}}");

        return string.Join(newLine, lines);
    }

    [GeneratedRegex(@"children\s*:\s*\[")]
    private static partial Regex ChildrenArrayRegex();
}

public sealed record RouteInsertionResult(RouteInsertionStatus Status, string? Content = null)
{
    public static RouteInsertionResult Inserted(string content) => new(RouteInsertionStatus.Inserted, content);
    public static RouteInsertionResult Duplicate => new(RouteInsertionStatus.Duplicate);
    public static RouteInsertionResult ChildrenNotFound => new(RouteInsertionStatus.ChildrenNotFound);
}

public enum RouteInsertionStatus
{
    Inserted,
    Duplicate,
    ChildrenNotFound
}
