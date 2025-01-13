namespace Oip.Data.Dtos;

/// <summary>
/// Feature Instance Dto
/// </summary>
public class FeatureInstanceDto
{
    /// <summary></summary>
    public int FeatureInstanceId { get; init; }

    /// <summary></summary>
    public int FeatureId { get; init; }

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
    public List<FeatureInstanceDto>? Items { get; init; }
}