namespace Oip.Rtds.Data.Entities;

/// <summary>
/// Represents an interface entity in the RTDS system
/// </summary>
public class InterfaceEntity
{
    /// <summary>
    /// Unique identifier for the interface
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// Name of the interface
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the interface
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Collection of tag entities associated with this interface
    /// </summary>
    public List<TagEntity>? Tags { get; set; } = new();
}