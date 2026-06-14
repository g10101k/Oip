namespace Oip.Api.Controllers.Api;

/// <summary>
/// Paginated data container holding a page of results with total item count.
/// </summary>
/// <typeparam name="T">Type of items contained in the data page</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Collection of items returned in paginated response
    /// </summary>
    public List<T>? Data { get; set; }

    /// <summary>
    /// Total number of records available after applying all filters
    /// </summary>
    public int Total { get; set; }
}