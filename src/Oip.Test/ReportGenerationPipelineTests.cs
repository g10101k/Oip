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
            SchemaVersion = 2,
            Id = "sales-report",
            Name = "Sales Report",
            CurrentVersion = 1,
            DataSourceKey = "main",
            Page = new ReportPageSettings
            {
                PaperFormat = ReportPaperFormat.A4,
                Orientation = ReportPageOrientation.Portrait,
                Width = 210,
                Height = 297,
                Unit = ReportMeasurementUnit.Millimeter,
                Margins = new ReportPageMargins { Top = 15, Right = 15, Bottom = 15, Left = 15 }
            },
            Exports =
            [
                new ReportExportDefinition
                {
                    Format = ReportExportFormat.Html,
                    FileNameTemplate = "sales-report.html",
                    Settings = []
                }
            ],
            Localization = new ReportLocalizationSettings
            {
                DefaultCulture = "en",
                SupportedCultures = ["en"],
                Resources = []
            },
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
                    Type = ReportBandType.ReportHeader,
                    Height = 20,
                    Elements = [new ReportElement { TextTemplate = "{{parameter:title}}", Layout = new ReportElementLayout { X = 0, Y = 0, Width = 180, Height = 10 } }]
                },
                new ReportBand
                {
                    Type = ReportBandType.Detail,
                    Height = 10,
                    Elements =
                    [
                        new ReportElement { Label = "Customer", ValuePath = "customer", Layout = new ReportElementLayout { X = 0, Y = 0, Width = 90, Height = 8 } },
                        new ReportElement { Label = "Amount", ValuePath = "amount", Format = "0.00", Align = "right", Layout = new ReportElementLayout { X = 100, Y = 0, Width = 80, Height = 8 } }
                    ]
                },
                new ReportBand
                {
                    Type = ReportBandType.ReportFooter,
                    Height = 15,
                    Elements = [new ReportElement { TextTemplate = "Rows: {{summary:count}} | Sum: {{summary:sum:amount}}", Layout = new ReportElementLayout { X = 0, Y = 0, Width = 180, Height = 8 } }]
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
        Assert.That(result.Document.Html, Does.Contain("left:100mm"));
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
