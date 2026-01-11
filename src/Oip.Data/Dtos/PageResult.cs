namespace Oip.Data.Dtos;

/// <summary>
/// Paged result set containing a list of items along with pagination metadata
/// </summary>
public record PageResult<T>(List<T> Results, int TotalPages, int CurrentPage);