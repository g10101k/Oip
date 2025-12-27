
using Oip.Rtds.Grpc;

namespace Oip.Rtds.Data.Entities;

/// <summary>
/// Represents the configuration and metadata of a tag.
/// </summary>
public class TagEntity
{
    /// <summary>
    /// Unique identifier of the tag.
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// Name of the tag.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Data type of the point (e.g., Float32, Int32, Digital, String, Blob).
    /// </summary>
    public TagTypes ValueType { get; set; }

    /// <summary>
    /// The interface associated with the tag.
    /// </summary>
    public uint? InterfaceId { get; set; }

    /// <summary>
    /// Represents the interface associated with a tag.
    /// </summary>
    public virtual InterfaceEntity? Interface { get; set; }

    /// <summary>
    /// Description of the point (used as a comment or label).
    /// </summary>
    public string? Descriptor { get; set; }

    /// <summary>
    /// Engineering units (e.g., °C, PSI, m³/h).
    /// </summary>
    public string? Uom { get; set; }

    /// <summary>
    /// Reference to the source signal or channel tag.
    /// </summary>
    public string? InstrumentTag { get; set; }

    /// <summary>
    /// Indicates whether the point is archived.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Indicates whether compression is enabled for this tag.
    /// </summary>
    public bool Compressing { get; set; } = false;

    /// <summary>
    /// Minimum time (in milliseconds) between compressed values.
    /// Values received within this period are discarded, regardless of their error margin.
    /// </summary>
    public uint? CompressionMinTime { get; set; }

    /// <summary>
    /// Maximum time (in milliseconds) between compressed values.
    /// </summary>
    public uint? CompressionMaxTime { get; set; }

    /// <summary>
    /// Associated digital state set name (for digital-type points).
    /// </summary>
    public string? DigitalSet { get; set; }

    /// <summary>
    /// Indicates whether values are treated as step (true) or interpolated (false).
    /// </summary>
    public bool Step { get; set; }

    /// <summary>
    /// Formula used to calculate the time associated with the tag's value.
    /// Default `now()`;
    /// </summary>
    public string? TimeCalculation { get; set; }

    /// <summary>
    /// Formula used to calculate error values for the tag.
    /// </summary>
    public string? ErrorCalculation { get; set; }

    /// <summary>
    /// User-defined calculation or formula associated with the tag's value.
    /// </summary>
    /// <remarks>
    /// This property allows for custom calculations or transformations to be applied to the raw tag value.
    /// It can contain any valid expression or formula, depending on the RTDS configuration.
    /// </remarks>
    public string? ValueCalculation { get; set; }

    /// <summary>
    /// Date and time when the tag was created.
    /// </summary>
    public DateTimeOffset CreationDate { get; set; }

    /// <summary>
    /// User or process that created the tag.
    /// </summary>
    public string Creator { get; set; } = string.Empty;
}