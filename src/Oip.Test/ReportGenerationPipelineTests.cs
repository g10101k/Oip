using Oip.Reports.Base.Abstractions;
using Oip.Reports.Base.Models;
using Oip.Reports.Base.Pipeline;
using Oip.Reports.Base.Rendering;

namespace Oip.Test;

[TestFixture]
public class ReportGenerationPipelineTests
{
    private InMemoryReportStorageService _storageService = null!;
    private StaticReportDataProvider _dataProvider = null!;
    private ReportGenerationPipeline _pipeline = null!;

    [SetUp]
    public void SetUp()
    {
        var definition = new ReportDefinition
        {
            Id = "sales-report",
            Name = "Sales Report",
            CurrentVersion = 1,
            DataSourceKey = "main",
            Parameters =
            [
                new ReportParameterDefinition
                {
                    Name = "title",
                    Required = true,
                    DefaultValue = "Sales Report"
                }
            ],
            DataSources =
            [
                new ReportDataSource
                {
                    Key = "main",
                    ProviderKey = "test"
                }
            ],
            Bands =
            [
                new ReportBand
                {
                    Type = ReportBandType.Header,
                    Elements = [new ReportElement { TextTemplate = "{{parameter:title}}" }]
                },
                new ReportBand
                {
                    Type = ReportBandType.Detail,
                    Elements =
                    [
                        new ReportElement { Label = "Customer", ValuePath = "customer" },
                        new ReportElement { Label = "Amount", ValuePath = "amount", Format = "0.00", Align = "right" }
                    ]
                },
                new ReportBand
                {
                    Type = ReportBandType.Footer,
                    Elements = [new ReportElement { TextTemplate = "Rows: {{summary:count}} | Sum: {{summary:sum:amount}}" }]
                }
            ]
        };

        _storageService = new InMemoryReportStorageService(definition);
        _dataProvider = new StaticReportDataProvider(
        [
            new Dictionary<string, object?> { ["customer"] = "Acme", ["amount"] = 12.5m },
            new Dictionary<string, object?> { ["customer"] = "Globex", ["amount"] = 7.5m }
        ]);
        _pipeline = new ReportGenerationPipeline(_storageService, _dataProvider, new DefaultReportLayoutStrategy(), new HtmlReportRenderer());
    }

    [Test]
    public async Task GenerateAsync_RendersHtmlPreview()
    {
        var result = await _pipeline.GenerateAsync(new ReportRequest
        {
            ReportId = "sales-report",
            Parameters = new Dictionary<string, string?> { ["title"] = "Quarterly Revenue" }
        });

        Assert.That(result.Document, Is.Not.Null);
        Assert.That(result.Document!.Html, Does.Contain("Quarterly Revenue"));
        Assert.That(result.Document.Html, Does.Contain("Acme"));
        Assert.That(result.Document.Html, Does.Contain("Rows: 2 | Sum: 20"));
    }

    [Test]
    public void GenerateAsync_WhenRequiredParameterMissing_Throws()
    {
        Assert.ThrowsAsync<InvalidOperationException>(() => _pipeline.GenerateAsync(new ReportRequest
        {
            ReportId = "sales-report",
            Parameters = new Dictionary<string, string?> { ["title"] = "" }
        }));
    }

    [Test]
    public void BuildCacheKey_UsesDefinitionVersionParametersAndUserContext()
    {
        var request = new ReportRequest
        {
            ReportId = "sales-report",
            Version = 4,
            Parameters = new Dictionary<string, string?> { ["title"] = "Revenue" },
            UserContext = new Dictionary<string, string?> { ["userId"] = "42" }
        };

        var cacheKey = ReportGeneratorUtils.BuildCacheKey(request);

        Assert.That(cacheKey, Is.EqualTo("sales-report:4:Html:title=Revenue:userId=42"));
    }

    private sealed class InMemoryReportStorageService(ReportDefinition definition) : IReportStorageService
    {
        public Task<IReadOnlyCollection<ReportDefinitionSummary>> GetDefinitionsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<ReportDefinitionSummary>>(
            [
                new ReportDefinitionSummary { Id = definition.Id, Name = definition.Name, CurrentVersion = definition.CurrentVersion }
            ]);

        public Task<ReportDefinition?> GetDefinitionAsync(string reportId, CancellationToken cancellationToken = default) =>
            Task.FromResult<ReportDefinition?>(definition.Id == reportId ? definition : null);

        public Task<ReportTemplateVersion?> GetTemplateVersionAsync(string reportId, int? version = null, CancellationToken cancellationToken = default) =>
            Task.FromResult<ReportTemplateVersion?>(definition.Id == reportId
                ? new ReportTemplateVersion { ReportId = definition.Id, Version = version ?? definition.CurrentVersion, Definition = definition }
                : null);

        public Task<ReportDefinition> CreateDefinitionAsync(ReportDefinition reportDefinition, CancellationToken cancellationToken = default) =>
            Task.FromResult(reportDefinition);

        public Task<ReportDefinition> UpdateDefinitionAsync(string reportId, ReportDefinition reportDefinition, CancellationToken cancellationToken = default) =>
            Task.FromResult(reportDefinition);

        public Task DeleteDefinitionAsync(string reportId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class StaticReportDataProvider(List<Dictionary<string, object?>> rows) : IReportDataProvider
    {
        public Task<ReportDataSet> GetDataAsync(ReportContext context, ReportDataSource dataSource, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ReportDataSet
            {
                Rows = rows
            });
        }
    }
}
