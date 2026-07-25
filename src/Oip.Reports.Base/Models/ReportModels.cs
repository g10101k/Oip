using System.Text.Json.Serialization;

namespace Oip.Reports.Base.Models;

public class ReportDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? CurrentVersionLabel { get; set; }
    public int CurrentVersion { get; set; } = 1;
    public string DataSourceKey { get; set; } = string.Empty;
    public List<ReportParameterDefinition> Parameters { get; set; } = [];
    public List<ReportDataSource> DataSources { get; set; } = [];
    public List<ReportStyle> Styles { get; set; } = [];
    public List<ReportBand> Bands { get; set; } = [];
    public List<ReportExportFormat> SupportedFormats { get; set; } = [ReportExportFormat.Html];
    public Dictionary<string, string> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class ReportTemplateVersion
{
    public string ReportId { get; set; } = string.Empty;
    public int Version { get; set; }
    public string VersionLabel { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system";
    public ReportDefinition Definition { get; set; } = new();
}

public class ReportParameterDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public ReportParameterType Type { get; set; } = ReportParameterType.String;
    public bool Required { get; set; }
    public string? DefaultValue { get; set; }
    public string? Description { get; set; }
}

public class ReportRequest
{
    public string ReportId { get; set; } = string.Empty;
    public int? Version { get; set; }
    public ReportExportFormat Format { get; set; } = ReportExportFormat.Html;
    public Dictionary<string, string?> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string?> UserContext { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class ReportResult
{
    public string JobId { get; set; } = Guid.NewGuid().ToString("N");
    public string ReportId { get; set; } = string.Empty;
    public int Version { get; set; }
    public ReportJobStatus Status { get; set; } = ReportJobStatus.Completed;
    public bool IsCached { get; set; }
    public ReportDocument? Document { get; set; }
    public ReportExecutionLog ExecutionLog { get; set; } = new();
}

public class ReportDocument
{
    public string FileName { get; set; } = "report.html";
    public string ContentType { get; set; } = "text/html; charset=utf-8";
    public string Html { get; set; } = string.Empty;
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
    public string CacheKey { get; set; } = string.Empty;
}

public class ReportExportResult
{
    public string JobId { get; set; } = Guid.NewGuid().ToString("N");
    public string ReportId { get; set; } = string.Empty;
    public int Version { get; set; }
    public ReportJobStatus Status { get; set; } = ReportJobStatus.Completed;
    public bool IsCached { get; set; }
    public string FileName { get; set; } = "report.html";
    public string ContentType { get; set; } = "text/html; charset=utf-8";
    public string ContentBase64 { get; set; } = string.Empty;
    public ReportExecutionLog ExecutionLog { get; set; } = new();
}

public class ReportExecutionLog
{
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime FinishedAtUtc { get; set; } = DateTime.UtcNow;
    public string? DefinitionSource { get; set; }
    public string? CacheKey { get; set; }
    public int RowCount { get; set; }
    public List<string> Steps { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
}

public class ReportDataSource
{
    public string Key { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public Dictionary<string, string> Query { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class ReportStyle
{
    public string Id { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public Dictionary<string, string> Properties { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class ReportBand
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public ReportBandType Type { get; set; }
    public string? Title { get; set; }
    public string? StyleId { get; set; }
    public List<ReportElement> Elements { get; set; } = [];
}

public class ReportElement
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public ReportElementType Type { get; set; } = ReportElementType.Text;
    public string? Label { get; set; }
    public string? TextTemplate { get; set; }
    public string? ValuePath { get; set; }
    public string? Format { get; set; }
    public string? StyleId { get; set; }
    public string? Width { get; set; }
    public string? Align { get; set; }
    public bool AllowHtml { get; set; }
}

public class ReportContext
{
    public ReportTemplateVersion TemplateVersion { get; init; } = new();
    public ReportRequest Request { get; init; } = new();
    public IReadOnlyDictionary<string, string?> Parameters { get; init; } = new Dictionary<string, string?>();
    public IReadOnlyDictionary<string, string?> UserContext { get; init; } = new Dictionary<string, string?>();
    public ReportDataSet DataSet { get; init; } = new();
}

public class ReportDataSet
{
    public List<Dictionary<string, object?>> Rows { get; set; } = [];
}

public class ReportLayout
{
    public string Title { get; set; } = string.Empty;
    public List<ReportLayoutSection> Sections { get; set; } = [];
}

public class ReportLayoutSection
{
    public ReportBandType Type { get; set; }
    public string? CssClass { get; set; }
    public List<ReportLayoutRow> Rows { get; set; } = [];
}

public class ReportLayoutRow
{
    public List<ReportLayoutCell> Cells { get; set; } = [];
}

public class ReportLayoutCell
{
    public string Text { get; set; } = string.Empty;
    public string? CssClass { get; set; }
    public string? Width { get; set; }
    public string? Align { get; set; }
    public bool IsHtml { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReportBandType
{
    Header,
    Detail,
    Footer
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReportElementType
{
    Text,
    Value
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReportParameterType
{
    String,
    Number,
    Date,
    Boolean
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReportExportFormat
{
    Html
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReportJobStatus
{
    Pending,
    Running,
    Completed,
    Failed
}

public class ReportDefinitionSummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CurrentVersion { get; set; }
    public string DataSourceKey { get; set; } = string.Empty;
}
