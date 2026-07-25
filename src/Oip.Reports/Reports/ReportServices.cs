using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Oip.Demo.TableQueryDemo;
using Oip.Reports.Base.Abstractions;
using Oip.Reports.Base.Models;
using Oip.Reports.Base.Pipeline;
using Oip.Reports.Base.Rendering;

namespace Oip.Reports.Reports;

public sealed class DiskReportStorageService(IWebHostEnvironment environment) : IReportStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private string RootPath => Path.Combine(environment.ContentRootPath, "Reports", "Templates");

    public async Task<IReadOnlyCollection<ReportDefinitionSummary>> GetDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(RootPath);
        var result = new List<ReportDefinitionSummary>();
        foreach (var file in Directory.EnumerateFiles(RootPath, "*.json", SearchOption.TopDirectoryOnly))
        {
            var definition = await ReadDefinitionAsync(file, cancellationToken);
            result.Add(new ReportDefinitionSummary
            {
                Id = definition.Id,
                Name = definition.Name,
                Description = definition.Description,
                CurrentVersion = definition.CurrentVersion,
                DataSourceKey = definition.DataSourceKey
            });
        }

        return result.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public async Task<ReportDefinition?> GetDefinitionAsync(string reportId, CancellationToken cancellationToken = default)
    {
        var path = GetPath(reportId);
        return File.Exists(path) ? await ReadDefinitionAsync(path, cancellationToken) : null;
    }

    public async Task<ReportTemplateVersion?> GetTemplateVersionAsync(string reportId, int? version = null, CancellationToken cancellationToken = default)
    {
        var definition = await GetDefinitionAsync(reportId, cancellationToken);
        if (definition is null)
            return null;

        var resolvedVersion = version ?? definition.CurrentVersion;
        return new ReportTemplateVersion
        {
            ReportId = definition.Id,
            Version = resolvedVersion,
            VersionLabel = definition.CurrentVersionLabel ?? $"v{resolvedVersion}",
            Definition = definition
        };
    }

    public async Task<ReportDefinition> CreateDefinitionAsync(ReportDefinition definition, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(RootPath);
        NormalizeDefinition(definition, true);
        await WriteDefinitionAsync(GetPath(definition.Id), definition, cancellationToken);
        return definition;
    }

    public async Task<ReportDefinition> UpdateDefinitionAsync(string reportId, ReportDefinition definition, CancellationToken cancellationToken = default)
    {
        NormalizeDefinition(definition, false);
        definition.Id = reportId;
        definition.CurrentVersion++;
        definition.CurrentVersionLabel = $"v{definition.CurrentVersion}";
        await WriteDefinitionAsync(GetPath(reportId), definition, cancellationToken);
        return definition;
    }

    public Task DeleteDefinitionAsync(string reportId, CancellationToken cancellationToken = default)
    {
        var path = GetPath(reportId);
        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    private static void NormalizeDefinition(ReportDefinition definition, bool isNew)
    {
        definition.Id = string.IsNullOrWhiteSpace(definition.Id)
            ? ToSlug(definition.Name)
            : ToSlug(definition.Id);

        if (isNew)
        {
            definition.CurrentVersion = Math.Max(definition.CurrentVersion, 1);
            definition.CurrentVersionLabel ??= $"v{definition.CurrentVersion}";
        }
    }

    private async Task<ReportDefinition> ReadDefinitionAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<ReportDefinition>(stream, JsonOptions, cancellationToken)
               ?? throw new InvalidOperationException($"Could not deserialize report definition '{path}'.");
    }

    private async Task WriteDefinitionAsync(string path, ReportDefinition definition, CancellationToken cancellationToken)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, definition, JsonOptions, cancellationToken);
    }

    private string GetPath(string reportId) => Path.Combine(RootPath, $"{ToSlug(reportId)}.json");

    private static string ToSlug(string value)
    {
        var slug = value.Trim().ToLowerInvariant().Replace(' ', '-');
        return string.Concat(slug.Where(x => char.IsLetterOrDigit(x) || x == '-'));
    }
}

public sealed class DemoCustomerReportDataProvider(DemoCustomerTableContext context) : IReportDataProvider
{
    public async Task<ReportDataSet> GetDataAsync(ReportContext reportContext, ReportDataSource dataSource, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(dataSource.ProviderKey, "ef-demo-customer", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Provider '{dataSource.ProviderKey}' is not supported.");

        var category = reportContext.Parameters.GetValueOrDefault("category");
        var includeInactive = bool.TryParse(reportContext.Parameters.GetValueOrDefault("includeInactive"), out var parsedIncludeInactive)
            && parsedIncludeInactive;
        var minLifetimeValue = decimal.TryParse(reportContext.Parameters.GetValueOrDefault("minLifetimeValue"), out var parsedMinLifetimeValue)
            ? parsedMinLifetimeValue
            : 0m;

        var query = context.Customers
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Country)
            .Include(x => x.Orders)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(x => x.Category.Name == category);

        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        query = query.Where(x => x.LifetimeValue >= minLifetimeValue)
            .OrderByDescending(x => x.LifetimeValue)
            .ThenBy(x => x.LastName)
            .ThenBy(x => x.FirstName);

        var customers = await query
            .Select(customer => new
            {
                FullName = customer.FirstName + " " + customer.LastName,
                customer.Email,
                Category = customer.Category.Name,
                Country = customer.Country.Name,
                Status = customer.Status.ToString(),
                OrdersCount = customer.Orders.Count,
                customer.LifetimeValue,
                customer.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var rows = customers.Select(customer => new Dictionary<string, object?>
        {
            ["fullName"] = customer.FullName,
            ["email"] = customer.Email,
            ["category"] = customer.Category,
            ["country"] = customer.Country,
            ["status"] = customer.Status,
            ["ordersCount"] = customer.OrdersCount,
            ["lifetimeValue"] = customer.LifetimeValue,
            ["createdAt"] = customer.CreatedAt
        }).ToList();

        return new ReportDataSet
        {
            Rows = rows
        };
    }
}

public sealed class ReportDefinitionService(IReportStorageService storageService) : IReportDefinitionService
{
    public Task<IReadOnlyCollection<ReportDefinitionSummary>> GetReportsAsync(CancellationToken cancellationToken = default) =>
        storageService.GetDefinitionsAsync(cancellationToken);

    public Task<ReportDefinition?> GetReportByIdAsync(string reportId, CancellationToken cancellationToken = default) =>
        storageService.GetDefinitionAsync(reportId, cancellationToken);

    public Task<ReportDefinition> CreateReportAsync(ReportDefinition definition, CancellationToken cancellationToken = default) =>
        storageService.CreateDefinitionAsync(definition, cancellationToken);

    public Task<ReportDefinition> UpdateReportAsync(string reportId, ReportDefinition definition, CancellationToken cancellationToken = default) =>
        storageService.UpdateDefinitionAsync(reportId, definition, cancellationToken);

    public Task DeleteReportAsync(string reportId, CancellationToken cancellationToken = default) =>
        storageService.DeleteDefinitionAsync(reportId, cancellationToken);
}

public sealed class CachedReportGenerationService(
    IReportGenerationPipeline pipeline,
    IMemoryCache memoryCache) : IReportGenerationService
{
    public async Task<ReportResult> GetPreviewAsync(ReportRequest request, CancellationToken cancellationToken = default)
    {
        var cacheKey = ReportGeneratorUtils.BuildCacheKey(request);
        if (memoryCache.TryGetValue(cacheKey, out ReportResult? cached) && cached is not null)
            return Clone(cached, true);

        var result = await pipeline.GenerateAsync(request, cancellationToken);
        result.ExecutionLog.CacheKey = cacheKey;
        memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(15));
        return Clone(result, false);
    }

    private static ReportResult Clone(ReportResult source, bool isCached)
    {
        return new ReportResult
        {
            JobId = Guid.NewGuid().ToString("N"),
            ReportId = source.ReportId,
            Version = source.Version,
            Status = source.Status,
            IsCached = isCached,
            Document = source.Document,
            ExecutionLog = source.ExecutionLog
        };
    }
}

public sealed class ReportExportService(
    IReportGenerationService generationService,
    IEnumerable<IReportExporter> exporters) : IReportExportService
{
    public async Task<ReportExportResult> ExportAsync(ReportRequest request, CancellationToken cancellationToken = default)
    {
        var preview = await generationService.GetPreviewAsync(request, cancellationToken);
        var exporter = exporters.FirstOrDefault(x => x.Format == request.Format)
                       ?? throw new InvalidOperationException($"Format '{request.Format}' is not supported.");

        return exporter.Export(preview);
    }
}

public static class ReportServiceCollectionExtensions
{
    public static IServiceCollection AddReportServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<IReportStorageService, DiskReportStorageService>();
        services.AddScoped<IReportDataProvider, DemoCustomerReportDataProvider>();
        services.AddScoped<IReportLayoutStrategy, DefaultReportLayoutStrategy>();
        services.AddScoped<IReportDocumentRenderer, HtmlReportRenderer>();
        services.AddScoped<IReportExporter, HtmlReportExporter>();
        services.AddScoped<IReportGenerationPipeline, ReportGenerationPipeline>();
        services.AddScoped<IReportDefinitionService, ReportDefinitionService>();
        services.AddScoped<IReportGenerationService, CachedReportGenerationService>();
        services.AddScoped<IReportExportService, ReportExportService>();
        return services;
    }
}
