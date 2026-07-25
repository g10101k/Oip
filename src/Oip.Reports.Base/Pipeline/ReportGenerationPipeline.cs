using Oip.Reports.Base.Abstractions;
using Oip.Reports.Base.Models;

namespace Oip.Reports.Base.Pipeline;

public class ReportGenerationPipeline(
    IReportStorageService storageService,
    IReportDataProvider dataProvider,
    IReportLayoutStrategy layoutStrategy,
    IReportDocumentRenderer documentRenderer)
    : IReportGenerationPipeline
{
    public async Task<ReportResult> GenerateAsync(ReportRequest request, CancellationToken cancellationToken = default)
    {
        var log = new ReportExecutionLog
        {
            StartedAtUtc = DateTime.UtcNow
        };

        log.Steps.Add("load-template");
        var templateVersion = await storageService.GetTemplateVersionAsync(request.ReportId, request.Version, cancellationToken)
                              ?? throw new InvalidOperationException($"Report '{request.ReportId}' was not found.");

        ValidateParameters(templateVersion.Definition, request.Parameters);
        log.DefinitionSource = $"{templateVersion.ReportId}:v{templateVersion.Version}";
        log.Steps.Add("validate-parameters");

        var dataSource = templateVersion.Definition.DataSources.FirstOrDefault(x =>
                             x.Key.Equals(templateVersion.Definition.DataSourceKey, StringComparison.OrdinalIgnoreCase))
                         ?? throw new InvalidOperationException(
                             $"Data source '{templateVersion.Definition.DataSourceKey}' was not configured.");

        var contextSkeleton = new ReportContext
        {
            TemplateVersion = templateVersion,
            Request = request,
            Parameters = new Dictionary<string, string?>(request.Parameters, StringComparer.OrdinalIgnoreCase),
            UserContext = new Dictionary<string, string?>(request.UserContext, StringComparer.OrdinalIgnoreCase)
        };

        log.Steps.Add("resolve-data");
        var dataSet = await dataProvider.GetDataAsync(contextSkeleton, dataSource, cancellationToken);

        var context = new ReportContext
        {
            TemplateVersion = templateVersion,
            Request = request,
            Parameters = contextSkeleton.Parameters,
            UserContext = contextSkeleton.UserContext,
            DataSet = dataSet
        };

        log.Steps.Add("build-layout");
        var layout = layoutStrategy.BuildLayout(context);
        log.Steps.Add("render-document");

        var cacheKey = ReportGeneratorUtils.BuildCacheKey(request);
        var document = documentRenderer.Render(context, layout, cacheKey);

        log.CacheKey = cacheKey;
        log.RowCount = dataSet.Rows.Count;
        log.FinishedAtUtc = DateTime.UtcNow;

        return new ReportResult
        {
            ReportId = templateVersion.ReportId,
            Version = templateVersion.Version,
            Status = ReportJobStatus.Completed,
            Document = document,
            ExecutionLog = log
        };
    }

    private static void ValidateParameters(ReportDefinition definition, IReadOnlyDictionary<string, string?> values)
    {
        var missing = definition.Parameters
            .Where(x => x.Required && string.IsNullOrWhiteSpace(values.GetValueOrDefault(x.Name) ?? x.DefaultValue))
            .Select(x => x.Name)
            .ToArray();

        if (missing.Length > 0)
            throw new InvalidOperationException($"Missing required report parameters: {string.Join(", ", missing)}.");
    }
}
