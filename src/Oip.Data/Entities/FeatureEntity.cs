namespace Oip.Data.Entities;

/// <summary>
/// It features in app
/// </summary>
public class FeatureEntity
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
}