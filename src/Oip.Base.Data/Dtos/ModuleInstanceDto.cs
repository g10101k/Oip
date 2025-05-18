namespace Oip.Base.Data.Dtos;

/// <summary>
/// Module Instance Dto
/// </summary>
public class ModuleInstanceDto
{
    /// <summary></summary>
    public int ModuleInstanceId { get; init; }

    /// <summary></summary>
    public int ModuleId { get; init; }

    /// <summary></summary>
    public string Label { get; init; } = null!;

    /// <summary></summary>
    public string? Icon { get; init; }

    /// <summary></summary>
    public List<string>? RouterLink { get; init; }

    /// <summary></summary>
    public string? Url { get; init; }

    /// <summary></summary>
    public string? Target { get; init; }

    /// <summary></summary>
    public string? Settings { get; init; }

    /// <summary>Childs</summary>
    public List<ModuleInstanceDto>? Items { get; init; }
}