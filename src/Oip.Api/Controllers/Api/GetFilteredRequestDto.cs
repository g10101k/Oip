namespace Oip.Api.Controllers.Api;

/// <summary>
/// Data transfer object for filtered queries with pagination and sorting support.
/// Generic type parameter allows specifying filter criteria structure.
/// </summary>
/// <typeparam name="T">Filter criteria type defining query parameters</typeparam>
public class GetFilteredRequestDto<T> where T : class
{
    /// <summary>
    /// Filter criteria applied to query results
    /// </summary>
    public T Filter { get; set; } = null!;

    /// <summary>
    /// Number of records to retrieve starting from the first result
    /// </summary>
    public int First { get; set; }

    /// <summary>
    /// Number of rows to retrieve
    /// </summary>
    public int Rows { get; set; }

    /// <summary>
    /// Field name to use for sorting results
    /// </summary>
    public string? SortField { get; set; }

    /// <summary>
    /// Sort direction indicator
    /// </summary>
    public int SortOrder { get; set; }
}