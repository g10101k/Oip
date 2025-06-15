using Oip.Rtds.Data.Enums;

namespace Oip.Rtds.Data.Dtos;

/// <summary>
/// Represents the configuration and metadata of a tag.
/// </summary>
public class TagGetDto
{
    /// <summary>
    /// Unique identifier of the tag.
    /// </summary>
    public uint TagId { get; set; }

    /// <summary>
    /// Name of the tag.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Data type of the point (e.g., Float32, Int32, Digital, String, Blob).
    /// </summary>
    public TagTypes ValueType { get; set; }

    /// <summary>
    /// Single-letter code indicating the point source (e.g., 'R', 'L').
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Description of the point (used as a comment or label).
    /// </summary>
    public string? Descriptor { get; set; }

    /// <summary>
    /// Engineering units (e.g., °C, PSI, m³/h).
    /// </summary>
    public string? EngUnits { get; set; }

    /// <summary>
    /// Reference to the source signal or channel tag.
    /// </summary>
    public string? InstrumentTag { get; set; }

    /// <summary>
    /// Indicates whether the point is archived.
    /// </summary>
    public bool? Archiving { get; set; }

    /// <summary>
    /// Indicates whether compression is enabled for this tag.
    /// </summary>
    public bool? Compressing { get; set; }

    /// <summary>
    /// Exception deviation: minimum change required to store a new value.
    /// </summary>
    public double? ExcDev { get; set; }

    /// <summary>
    /// Minimum time (in seconds) between archived values.
    /// </summary>
    public int? ExcMin { get; set; }

    /// <summary>
    /// Maximum time (in seconds) between archived values.
    /// </summary>
    public int? ExcMax { get; set; }

    /// <summary>
    /// Compression deviation: minimum change required to pass compression filter.
    /// </summary>
    public double? CompDev { get; set; }

    /// <summary>
    /// Minimum time (in seconds) between compressed values.
    /// </summary>
    public int? CompMin { get; set; }

    /// <summary>
    /// Maximum time (in seconds) between compressed values.
    /// </summary>
    public int? CompMax { get; set; }

    /// <summary>
    /// The minimum expected value of the signal.
    /// </summary>
    public double Zero { get; set; } = 0.0d;

    /// <summary>
    /// The range between the zero and the maximum value.
    /// </summary>
    public double Span { get; set; } = 100.0d;

    /// <summary>
    /// The range between the zero and the maximum value.
    /// </summary>
    public double TypicalValue { get; set; }  = 50.0d;

    /// <summary>
    /// Extended description, often used by interfaces.
    /// </summary>
    public string? ExDesc { get; set; }

    /// <summary>
    /// Indicates whether the point is being scanned by the interface.
    /// </summary>
    public bool? Scan { get; set; }

    /// <summary>
    /// Associated digital state set name (for digital-type points).
    /// </summary>
    public string? DigitalSet { get; set; }

    /// <summary>
    /// Indicates whether values are treated as step (true) or interpolated (false).
    /// </summary>
    public bool Step { get; set; }

    /// <summary>
    /// Date and time when the tag was created.
    /// </summary>
    public DateTimeOffset CreationDate { get; set; }

    /// <summary>
    /// User or process that created the tag.
    /// </summary>
    public string Creator { get; set; } = string.Empty;

    /// <summary>
    /// ClickHouse partitioning clause for time-series storage (e.g., "PARTITION BY toYear(time)").
    /// Used to control how data is partitioned when creating the table.
    /// </summary>
    public string Partition { get; set; } = "PARTITION BY toYear(Time)";
}