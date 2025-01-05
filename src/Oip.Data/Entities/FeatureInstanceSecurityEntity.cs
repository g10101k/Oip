namespace Oip.Data.Entities;

/// <summary>
/// Feature instance security entity
/// </summary>
public class FeatureInstanceSecurityEntity
{
    /// <summary>
    /// Id
    /// </summary>
    public int FeatureInstanceSecurityId { get; set; }

    /// <summary>
    /// FeatureId
    /// </summary>
    public int FeatureInstanceId { get; set; }

    /// <summary>
    /// Right (max 255 chars)
    /// </summary>
    public string Right { get; set; } = null!;

    /// <summary>
    /// Role (max 255 chars)
    /// </summary>
    public string Role { get; set; } = null!;
    
    /// <summary>
    /// Feature Instance
    /// </summary>
    public FeatureInstanceEntity FeatureInstance { get; set; } = null!;
}