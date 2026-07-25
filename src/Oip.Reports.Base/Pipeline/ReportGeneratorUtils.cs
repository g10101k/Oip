using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Oip.Reports.Base.Models;

namespace Oip.Reports.Base.Pipeline;

public static partial class ReportGeneratorUtils
{
    [GeneratedRegex(@"\{\{\s*(parameter|row|summary)\s*:\s*([^}]+)\s*\}\}", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex TokenRegex();

    public static string ResolveTemplate(string? template, ReportContext context, IDictionary<string, object?>? row = null)
    {
        if (string.IsNullOrWhiteSpace(template))
            return string.Empty;

        return TokenRegex().Replace(template, match =>
        {
            var scope = match.Groups[1].Value.Trim().ToLowerInvariant();
            var payload = match.Groups[2].Value.Trim();

            return scope switch
            {
                "parameter" => context.Parameters.TryGetValue(payload, out var parameterValue) ? parameterValue ?? string.Empty : string.Empty,
                "row" => ResolveRowValue(payload, row),
                "summary" => ResolveSummaryValue(payload, context),
                _ => string.Empty
            };
        });
    }

    public static string ResolveElementValue(ReportElement element, ReportContext context, IDictionary<string, object?>? row = null)
    {
        var raw = !string.IsNullOrWhiteSpace(element.TextTemplate)
            ? ResolveTemplate(element.TextTemplate, context, row)
            : ResolveValuePath(element.ValuePath, row);

        return ApplyFormat(raw, element.Format);
    }

    public static string BuildCacheKey(ReportRequest request)
    {
        var parameterPart = string.Join("|", request.Parameters.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Select(x => $"{x.Key}={x.Value}"));
        var userContextPart = string.Join("|", request.UserContext.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Select(x => $"{x.Key}={x.Value}"));

        return $"{request.ReportId}:{request.Version?.ToString() ?? "current"}:{request.Format}:{parameterPart}:{userContextPart}";
    }

    public static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string ResolveRowValue(string payload, IDictionary<string, object?>? row)
    {
        if (row is null)
            return string.Empty;

        return ResolveValuePath(payload, row);
    }

    private static string ResolveSummaryValue(string payload, ReportContext context)
    {
        if (string.Equals(payload, "count", StringComparison.OrdinalIgnoreCase))
            return context.DataSet.Rows.Count.ToString(CultureInfo.InvariantCulture);

        var parts = payload.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2 && string.Equals(parts[0], "sum", StringComparison.OrdinalIgnoreCase))
        {
            var sum = context.DataSet.Rows.Sum(x => ToDecimal(x.TryGetValue(parts[1], out var value) ? value : null));
            return sum.ToString("0.##", CultureInfo.InvariantCulture);
        }

        return string.Empty;
    }

    private static string ResolveValuePath(string? valuePath, IDictionary<string, object?>? row)
    {
        if (string.IsNullOrWhiteSpace(valuePath) || row is null)
            return string.Empty;

        if (!row.TryGetValue(valuePath, out var value))
            return string.Empty;

        return value switch
        {
            null => string.Empty,
            DateTime dt => dt.ToString("u", CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString("u", CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
        };
    }

    private static string ApplyFormat(string value, string? format)
    {
        if (string.IsNullOrWhiteSpace(format))
            return value;

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
            return date.ToString(format, CultureInfo.InvariantCulture);

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
            return number.ToString(format, CultureInfo.InvariantCulture);

        return value;
    }

    private static decimal ToDecimal(object? value)
    {
        return value switch
        {
            null => 0m,
            decimal decimalValue => decimalValue,
            int intValue => intValue,
            long longValue => longValue,
            double doubleValue => Convert.ToDecimal(doubleValue, CultureInfo.InvariantCulture),
            float floatValue => Convert.ToDecimal(floatValue, CultureInfo.InvariantCulture),
            string stringValue when decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => 0m
        };
    }
}
