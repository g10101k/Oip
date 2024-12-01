using Oip.Data.Entities;

namespace Oip.Data.Dtos;

/// <summary>
/// It features in app
/// </summary>
public class FeatureDto
{
    /// <summary>
    /// Id
    /// </summary>
    public int FeatureId { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Settings
    /// </summary>
    public string? Settings { get; set; }

    public IEnumerable<FeatureSecurityDto> FeatureSecurities { get; set; } = null!;
}

public class FeatureSecurityDto
{
    public int FeatureSecurityId { get; set; }

    public string Role { get; set; }

    public string Right { get; set; }
}