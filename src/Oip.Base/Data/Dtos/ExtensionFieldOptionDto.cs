namespace Oip.Data.Dtos;

/// <summary>
/// Discrete option for select-like extension fields.
/// </summary>
public record ExtensionFieldOptionDto(
    string Value,
    string Label,
    string? Severity = null);
