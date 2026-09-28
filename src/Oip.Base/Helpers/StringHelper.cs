using System.Text;

namespace Oip.Base.Helpers;

/// <summary>
/// String helper
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// Converts a PascalCase or camelCase name to snake_case, e.g. <c>GetHTTPResponse2Async</c> to
    /// <c>get_http_response2_async</c>. An acronym is kept as one word; digits stay with the preceding word.
    /// </summary>
    /// <param name="value">The name to convert.</param>
    /// <return>The name in snake_case.</return>
    public static string ToSnakeCase(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var builder = new StringBuilder(value.Length + 8);
        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];
            if (char.IsUpper(current))
            {
                var previous = i > 0 ? value[i - 1] : '\0';
                var next = i + 1 < value.Length ? value[i + 1] : '\0';
                // A new word starts after a lowercase letter or a digit ("getName"), or at the last capital of an
                // acronym followed by a lowercase letter ("HTTPResponse" -> "http_response").
                var startsWord = char.IsLower(previous) || char.IsDigit(previous) ||
                                 (char.IsUpper(previous) && char.IsLower(next));
                if (startsWord && builder.Length > 0 && builder[^1] != '_')
                    builder.Append('_');
                builder.Append(char.ToLowerInvariant(current));
            }
            else
            {
                builder.Append(current);
            }
        }

        return builder.ToString();
    }
}
