using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Oip.Base.Data.Contexts;
using Oip.Base.Data.Repositories;
using Oip.Controllers;
using Oip.Demo.TableQueryDemo;
using Oip.Reports;
using Oip.Reports.Base.Abstractions;
using Oip.Reports.Base.Models;
using Oip.Reports.Base.Pipeline;
using Oip.Reports.Base.Rendering;
using Oip.Reports.Controllers;
using Oip.Reports.Reports;

namespace Oip.Test;

[TestFixture]
public class ReportModuleControllerTests
{
    private ServiceProvider _serviceProvider = null!;
    private string _contentRootPath = null!;

    [SetUp]
    public void SetUp()
    {
        _contentRootPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_contentRootPath, "Reports", "Templates"));
        File.WriteAllText(
            Path.Combine(_contentRootPath, "Reports", "Templates", "customer-directory.json"),
            """
            {
              "schemaVersion": 2,
              "id": "customer-directory",
              "name": "Customer Directory",
              "description": "Customer demo report",
              "currentVersion": 1,
              "dataSourceKey": "customers",
              "parameters": [
                { "name": "title", "label": "Title", "type": "String", "required": true, "defaultValue": "Customer Directory" },
                { "name": "includeInactive", "label": "Include Inactive", "type": "Boolean", "required": false, "defaultValue": "false" }
              ],
              "dataSources": [
                { "key": "customers", "providerKey": "ef-demo-customer", "query": {} }
              ],
              "styles": [],
              "bands": [
                { "type": "Header", "elements": [ { "type": "Text", "textTemplate": "{{parameter:title}}" } ] },
                { "type": "Detail", "elements": [ { "label": "Customer", "type": "Value", "valuePath": "fullName" } ] },
                { "type": "Footer", "elements": [ { "type": "Text", "textTemplate": "Rows: {{summary:count}}" } ] }
              ],
              "page": {
                "paperFormat": "A4",
                "orientation": "Portrait",
                "width": 210,
                "height": 297,
                "unit": "Millimeter",
                "margins": { "top": 15, "right": 15, "bottom": 15, "left": 15 }
              },
              "exports": [
                { "format": "Html", "fileNameTemplate": "customer-directory.html", "settings": {} }
              ],
              "localization": {
                "defaultCulture": "en",
                "supportedCultures": [ "en" ],
                "resources": {}
              }
            }
            """);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMemoryCache();
        services.AddDbContext<OipModuleContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString("N")));
        services.AddDbContext<DemoCustomerTableContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString("N")));
        services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment(_contentRootPath));
        services.AddScoped<ModuleRepository>();
        services.AddScoped<IReportStorageService, DiskReportStorageService>();
        services.AddScoped<IReportDataProvider, DemoCustomerReportDataProvider>();
        services.AddScoped<IReportLayoutStrategy, DefaultReportLayoutStrategy>();
        services.AddScoped<IReportDocumentRenderer, HtmlReportRenderer>();
        services.AddScoped<IReportExporter, HtmlReportExporter>();
        services.AddScoped<IReportGenerationPipeline, ReportGenerationPipeline>();
        services.AddScoped<IReportDefinitionService, ReportDefinitionService>();
        services.AddScoped<IReportGenerationService, CachedReportGenerationService>();
        services.AddScoped<IReportExportService, ReportExportService>();
        services.AddScoped<ReportModuleController>();

        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        DemoCustomerTableSeeder.Seed(scope.ServiceProvider.GetRequiredService<DemoCustomerTableContext>());
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider.Dispose();
        if (Directory.Exists(_contentRootPath))
            Directory.Delete(_contentRootPath, true);
    }

    [Test]
    public async Task GetReportPreview_ReturnsCompletedHtmlReport()
    {
        using var scope = _serviceProvider.CreateScope();
        var controller = scope.ServiceProvider.GetRequiredService<ReportModuleController>();

        var actionResult = await controller.GetReportPreview(new ReportRequest
        {
            ReportId = "customer-directory",
            Parameters = new Dictionary<string, string?>
            {
                ["title"] = "Preview Customers",
                ["includeInactive"] = "false"
            },
            UserContext = new Dictionary<string, string?> { ["timezone"] = "UTC" }
        });

        var okResult = actionResult.Result as OkObjectResult;
        var payload = okResult?.Value as ReportResult;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload!.Status, Is.EqualTo(ReportJobStatus.Completed));
        Assert.That(payload.Document?.Html, Does.Contain("Preview Customers"));
    }

    [Test]
    public async Task GetReportExport_ReturnsHtmlPayloadInBase64()
    {
        using var scope = _serviceProvider.CreateScope();
        var controller = scope.ServiceProvider.GetRequiredService<ReportModuleController>();

        var actionResult = await controller.GetReportExport(new ReportRequest
        {
            ReportId = "customer-directory",
            Format = ReportExportFormat.Html,
            Parameters = new Dictionary<string, string?>
            {
                ["title"] = "Export Customers"
            }
        });

        var okResult = actionResult.Result as OkObjectResult;
        var payload = okResult?.Value as ReportExportResult;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload!.ContentBase64, Is.Not.Empty);
    }

    [Test]
    public void Controller_UsesExpectedActionStyleRoutes()
    {
        var controllerType = typeof(ReportModuleController);

        Assert.Multiple(() =>
        {
            Assert.That(controllerType.GetMethod(nameof(ReportModuleController.CreateReport))?.GetCustomAttributes(typeof(HttpPostAttribute), false)
                .Cast<HttpPostAttribute>().Single().Template, Is.EqualTo("create-report"));
            Assert.That(controllerType.GetMethod(nameof(ReportModuleController.GetReportPreview))?.GetCustomAttributes(typeof(HttpPostAttribute), false)
                .Cast<HttpPostAttribute>().Single().Template, Is.EqualTo("get-report-preview"));
            Assert.That(controllerType.GetMethod(nameof(ReportModuleController.GetReportExport))?.GetCustomAttributes(typeof(HttpPostAttribute), false)
                .Cast<HttpPostAttribute>().Single().Template, Is.EqualTo("get-report-export"));
            Assert.That(controllerType.GetMethod(nameof(ReportModuleController.GetReportById))?.GetCustomAttributes(typeof(HttpGetAttribute), false)
                .Cast<HttpGetAttribute>().Single().Template, Is.EqualTo("get-report-by-id"));
            Assert.That(controllerType.GetMethod(nameof(ReportModuleController.UpdateReport))?.GetCustomAttributes(typeof(HttpPutAttribute), false)
                .Cast<HttpPutAttribute>().Single().Template, Is.EqualTo("update-report/{id}"));
            Assert.That(controllerType.GetMethod(nameof(ReportModuleController.DeleteReport))?.GetCustomAttributes(typeof(HttpDeleteAttribute), false)
                .Cast<HttpDeleteAttribute>().Single().Template, Is.EqualTo("delete-report/{id}"));
        });
    }

    private sealed class TestWebHostEnvironment(string contentRootPath) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Oip.Test";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = contentRootPath;
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(contentRootPath);
    }
}
