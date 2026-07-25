using Oip.Reports.Base.Models;

namespace Oip.Reports.Base.Abstractions;

public interface IReportStorageService
{
    Task<IReadOnlyCollection<ReportDefinitionSummary>> GetDefinitionsAsync(CancellationToken cancellationToken = default);
    Task<ReportDefinition?> GetDefinitionAsync(string reportId, CancellationToken cancellationToken = default);
    Task<ReportTemplateVersion?> GetTemplateVersionAsync(string reportId, int? version = null, CancellationToken cancellationToken = default);
    Task<ReportDefinition> CreateDefinitionAsync(ReportDefinition definition, CancellationToken cancellationToken = default);
    Task<ReportDefinition> UpdateDefinitionAsync(string reportId, ReportDefinition definition, CancellationToken cancellationToken = default);
    Task DeleteDefinitionAsync(string reportId, CancellationToken cancellationToken = default);
}

public interface IReportDataProvider
{
    Task<ReportDataSet> GetDataAsync(ReportContext context, ReportDataSource dataSource, CancellationToken cancellationToken = default);
}

public interface IReportLayoutStrategy
{
    ReportLayout BuildLayout(ReportContext context);
}

public interface IReportDocumentRenderer
{
    ReportDocument Render(ReportContext context, ReportLayout layout, string cacheKey);
}

public interface IReportExporter
{
    ReportExportFormat Format { get; }
    ReportExportResult Export(ReportResult reportResult);
}

public interface IReportGenerationPipeline
{
    Task<ReportResult> GenerateAsync(ReportRequest request, CancellationToken cancellationToken = default);
}

public interface IReportDefinitionService
{
    Task<IReadOnlyCollection<ReportDefinitionSummary>> GetReportsAsync(CancellationToken cancellationToken = default);
    Task<ReportDefinition?> GetReportByIdAsync(string reportId, CancellationToken cancellationToken = default);
    Task<ReportDefinition> CreateReportAsync(ReportDefinition definition, CancellationToken cancellationToken = default);
    Task<ReportDefinition> UpdateReportAsync(string reportId, ReportDefinition definition, CancellationToken cancellationToken = default);
    Task DeleteReportAsync(string reportId, CancellationToken cancellationToken = default);
}

public interface IReportGenerationService
{
    Task<ReportResult> GetPreviewAsync(ReportRequest request, CancellationToken cancellationToken = default);
}

public interface IReportExportService
{
    Task<ReportExportResult> ExportAsync(ReportRequest request, CancellationToken cancellationToken = default);
}
