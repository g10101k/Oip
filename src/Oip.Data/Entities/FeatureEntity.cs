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
    /// Settings for feature
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? Settings { get; set; }
    
    /// <summary>
    /// Route link to component
    /// </summary>
    public string? RouterLink { get; set; }
    
    /// <summary>
    /// Feature Securities
    /// </summary>
    public ICollection<FeatureSecurityEntity> FeatureSecurities { get; set; } = new List<FeatureSecurityEntity>();
    
    /// <summary>
    /// Instances
    /// </summary>
    public ICollection<FeatureInstanceEntity> FeatureInstances { get; set; } = new List<FeatureInstanceEntity>();
}