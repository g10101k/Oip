namespace Oip.Data.Entities;

/// <summary>
/// Feature security entity
/// </summary>
public class FeatureSecurityEntity
{
    /// <summary>
    /// Id
    /// </summary>
    public int FeatureSecurityId { get; set; }

    /// <summary>
    /// FeatureId
    /// </summary>
    public int FeatureId { get; set; }

    /// <summary>
    /// Right
    /// </summary>
    public string Right { get; set; } = null!;

    /// <summary>
    /// Role
    /// </summary>
    public string Role { get; set; } = null!;
}