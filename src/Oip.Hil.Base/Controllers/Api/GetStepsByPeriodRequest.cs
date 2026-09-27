namespace Oip.Hil.Base.Controllers.Api;

/// <summary>
/// Period of the workflows whose steps are listed.
/// </summary>
public class GetStepsByPeriodRequest
{
    /// <summary>
    /// Start of the period.
    /// </summary>
    public DateTimeOffset From { get; set; }

    /// <summary>
    /// End of the period.
    /// </summary>
    public DateTimeOffset To { get; set; }
}
