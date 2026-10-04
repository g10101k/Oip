using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Oip.Hitl.AiFunctions;
using Temporalio.Activities;
using Temporalio.Exceptions;

namespace Oip.Hitl.Workflows.Activities;

/// <summary>
/// Demo agent tools to try skills with.
/// </summary>
public partial class DemoToolActivities
{
    /// <summary>
    /// Arguments of <see cref="GetCurrentTime"/>.
    /// </summary>
    public class GetCurrentTimeArguments
    {
        /// <summary>
        /// IANA time zone.
        /// </summary>
        [JsonPropertyName("time_zone")]
        [Description("IANA time zone, e.g. Europe/Moscow. UTC when omitted.")]
        public string? TimeZone { get; init; }
    }

    /// <summary>
    /// Arguments of <see cref="EvaluateExpression"/>.
    /// </summary>
    public class EvaluateExpressionArguments
    {
        /// <summary>
        /// Arithmetic expression.
        /// </summary>
        [JsonPropertyName("expression")]
        [Description("Arithmetic expression with numbers, + - * / % and parentheses, e.g. (2 + 3) * 4.5.")]
        public string Expression { get; init; } = string.Empty;
    }

    /// <summary>
    /// Returns the current date and time in the time zone.
    /// </summary>
    [Activity, AgentTool("get_current_time", TimeoutSeconds = 10)]
    [Description("Returns the current date, time and day of the week in a time zone.")]
    public string GetCurrentTime(GetCurrentTimeArguments arguments)
    {
        var zone = TimeZoneInfo.Utc;
        if (!string.IsNullOrWhiteSpace(arguments.TimeZone) &&
            !TimeZoneInfo.TryFindSystemTimeZoneById(arguments.TimeZone.Trim(), out zone))
            throw new ApplicationFailureException($"Unknown time zone '{arguments.TimeZone}'", "InvalidArgument",
                nonRetryable: true);

        var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone);
        return now.ToString("yyyy-MM-dd HH:mm:ss zzz, dddd", CultureInfo.InvariantCulture) + $" ({zone.Id})";
    }

    /// <summary>
    /// Evaluates the arithmetic expression.
    /// </summary>
    [Activity, AgentTool("evaluate_expression", TimeoutSeconds = 10)]
    [Description("Evaluates an arithmetic expression exactly; use it instead of calculating in mind.")]
    public string EvaluateExpression(EvaluateExpressionArguments arguments)
    {
        // DataTable expressions also know functions and columns; only arithmetic is let through.
        if (string.IsNullOrWhiteSpace(arguments.Expression) || !ArithmeticPattern().IsMatch(arguments.Expression))
            throw new ApplicationFailureException("Only numbers, + - * / % and parentheses are allowed",
                "InvalidArgument", nonRetryable: true);
        try
        {
            var result = new DataTable().Compute(arguments.Expression, null);
            return Convert.ToString(result, CultureInfo.InvariantCulture) ?? string.Empty;
        }
        catch (Exception e) when (e is EvaluateException or SyntaxErrorException or OverflowException
                                      or DivideByZeroException)
        {
            throw new ApplicationFailureException(e.Message, "InvalidArgument", nonRetryable: true);
        }
    }

    [GeneratedRegex(@"^[\d\s.+\-*/%()]+$")]
    private static partial Regex ArithmeticPattern();
}
