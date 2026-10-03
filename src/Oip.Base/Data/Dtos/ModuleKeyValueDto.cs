namespace Oip.Base.Data.Dtos;

/// <summary>
/// Represents a module as a key-value pair with its default icon.
/// </summary>
/// <param name="Key">The module identifier.</param>
/// <param name="Value">The module name.</param>
/// <param name="Icon">The default icon for new module instances (optional).</param>
public record ModuleKeyValueDto(int Key, string Value, string? Icon);
