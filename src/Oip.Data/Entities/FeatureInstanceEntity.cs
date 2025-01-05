namespace Oip.Data.Entities;

/// <summary>
/// Feature instance
/// </summary>
public class FeatureInstanceEntity
{
    /// <summary>
    /// Id
    /// </summary>
    public int FeatureInstanceId { get; set; }

    /// <summary>
    /// Feature id
    /// </summary>
    public int FeatureId { get; set; }

    /// <summary>
    /// Label
    /// </summary>
    public string Label { get; set; } = null!;

    /// <summary>
    /// Label
    /// </summary>
    public string? Icon { get; set; } = null!;

    /// <summary>
    /// Url
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Target
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// Parent id
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Parent instance
    /// </summary>
    public FeatureInstanceEntity? Parent { get; set; }

    /// <summary>
    /// Children
    /// </summary>
    public List<FeatureInstanceEntity> Items { get; set; } = new();

    /// <summary>
    /// Securities
    /// </summary>
    public List<FeatureInstanceSecurityEntity> Securities { get; set; } = new();

    /// <summary>
    /// Settings
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Settings { get; set; } = null!;

    /// <summary>
    /// Feature
    /// </summary>
    public FeatureEntity Feature { get; set; } = null!;
}