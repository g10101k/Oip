namespace Oip.Api.Controllers.Api;

/// <summary>
/// Filter metadata container storing string-based criteria for query operations.
/// </summary>
public class FilterMetadataStringDto
{
    /// <summary>
    /// Filter value used for query matching
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Matching algorithm used for filtering string values
    /// </summary>
    public string? MatchMode { get; set; }

    /// <summary>
    /// Logical operator used for combining multiple filter conditions
    /// </summary>
    public string? Operator { get; set; }
}