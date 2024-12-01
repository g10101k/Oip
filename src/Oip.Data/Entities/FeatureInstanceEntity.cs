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
    /// Settings
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Settings { get; set; } = null!;
}