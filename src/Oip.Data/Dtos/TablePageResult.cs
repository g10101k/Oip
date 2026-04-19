namespace Oip.Data.Dtos;

/// <summary>
/// Standard response for server-side table data requests.
/// </summary>
public record TablePageResult<T>(List<T> Data, int Total, int First, int Rows);
