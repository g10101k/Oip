namespace Oip.Base.Api;

/// <summary>
/// Represents the configuration and metadata of tag
/// </summary>
public class Tag
{
    /// <summary>
    /// Id of the tag
    /// </summary>
    public uint TagId { get; set; }

    /// <summary>
    /// Tag name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Data type of the point (e.g., Float32, Int32, Digital, String, Blob).</summary>
    public string PointType { get; set; }

    /// <summary>
    /// Single-letter code indicating the point source (e.g., 'R', 'L').</summary>
    public string PointSource { get; set; }

    /// <summary>
    /// Description of the point (used as a comment or label).</summary>
    public string Descriptor { get; set; }

    /// <summary>
    /// Engineering units (e.g., °C, PSI, m³/h).</summary>
    public string EngUnits { get; set; }

    /// <summary>
    /// Reference to the source signal or channel tag.
    /// </summary>
    public string InstrumentTag { get; set; }

    /// <summary>
    /// Indicates whether the point is archived.
    /// </summary>
    public bool Archiving { get; set; } = true;

    /// <summary>
    /// Indicates whether compression is enabled for this tag
    /// </summary>
    public bool Compressing { get; set; } = true;
    
    /// <summary>
    /// Exception deviation: minimum change required to store a new value.
    /// </summary>
    public double ExcDev { get; set; } = 0.0d;

    /// <summary>
    /// Minimum time (in seconds) between archived values.
    /// </summary>
    public int ExcMin { get; set; } = 0;

    /// <summary>
    /// Maximum time (in seconds) between archived values.
    /// </summary>
    public int ExcMax { get; set; } = 600;

    /// <summary>
    /// Compression deviation: minimum change required to pass compression filter.
    /// </summary>
    public double CompDev { get; set; } = 0.0d;

    /// <summary>
    /// Minimum time (in seconds) between compressed values.
    /// </summary>
    public int CompMin { get; set; } = 0;

    /// <summary>
    /// Maximum time (in seconds) between compressed values.
    /// </summary>
    public int CompMax { get; set; } = 600;

    /// <summary>
    /// The minimum expected value of the signal.
    /// </summary>
    public int Zero { get; set; } = 0;

    /// <summary>
    /// The range between the zero and the maximum value.
    /// </summary>
    public int Span { get; set; } = 100;

    /// <summary>
    /// Interface-specific parameter: Location1 (usually the Interface ID).
    /// </summary>
    public int Location1 { get; set; } = 0;

    /// <summary>
    /// Interface-specific parameter: Location2.
    /// </summary>
    public int Location2 { get; set; } = 0;

    /// <summary>
    /// Interface-specific parameter: Location3.
    /// </summary>
    public int Location3 { get; set; } = 0;

    /// <summary>
    /// Interface-specific parameter: Location4.
    /// </summary>
    public int Location4 { get; set; } = 0;

    /// <summary>
    /// Interface-specific parameter: Location5.
    /// </summary>
    public int Location5 { get; set; } = 0;

    /// <summary>
    /// Extended description, often used by interfaces.
    /// </summary>
    public string ExDesc { get; set; }

    /// <summary>
    /// Indicates whether the point is being scanned by the interface.
    /// </summary>
    public bool Scan { get; set; } = true;

    /// <summary>
    /// Associated digital state set name (for digital-type points).
    /// </summary>
    public string DigitalSet { get; set; }

    /// <summary>
    /// Indicates whether values are treated as step (true) or interpolated (false).
    /// </summary>
    public bool Step { get; set; } = false;

    /// <summary>
    /// Indicates whether this point stores future (forecast) values.
    /// </summary>
    public bool Future { get; set; } = false;

    /// <summary>
    /// User-defined integer field #1.
    /// </summary>
    public int UserInt1 { get; set; }

    /// <summary>
    /// User-defined integer field #2.
    /// </summary>
    public int UserInt2 { get; set; }

    /// <summary>
    /// User-defined integer field #3.
    /// </summary>
    public int UserInt3 { get; set; }

    /// <summary>
    /// User-defined integer field #4.
    /// </summary>
    public int UserInt4 { get; set; }

    /// <summary>
    /// User-defined integer field #5.
    /// </summary>
    public int UserInt5 { get; set; }

    /// <summary>
    /// User-defined floating-point field #1.
    /// </summary>
    public double UserReal1 { get; set; }

    /// <summary>
    /// User-defined floating-point field #2.
    /// </summary>
    public double UserReal2 { get; set; }

    /// <summary>
    /// User-defined floating-point field #3.
    /// </summary>
    public double UserReal3 { get; set; }

    /// <summary>
    /// User-defined floating-point field #4.
    /// </summary>
    public double UserReal4 { get; set; }

    /// <summary>
    /// User-defined floating-point field #5.
    /// </summary>
    public double UserReal5 { get; set; }
    
    /// <summary>
    /// Date and time when the tag was created.
    /// </summary>
    public DateTime? CreationDate { get; set; }

    /// <summary>
    /// User or process that created the tag.
    /// </summary>
    public string Creator { get; set; }
}