using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Settings;
using Swashbuckle.AspNetCore.Swagger;

namespace Oip.Base.StartupTasks;

/// <summary>
/// Service responsible for executing tasks during application startup, specifically related to Swagger configuration.
/// </summary>
/// <param name="swaggerProvider">The Swagger provider.</param>
/// <param name="environment">The web host environment.</param>
/// <param name="logger">The logger.</param>
/// <param name="settings">The application settings.</param>
public class SwaggerGenerateWebClientStartupTask(
    ISwaggerProvider swaggerProvider,
    IWebHostEnvironment environment,
    ILogger<SwaggerGenerateWebClientStartupTask> logger,
    IBaseOipModuleAppSettings settings) : IStartupTask
{
    /// <inheritdoc />
    public int Order { get; } = 0;

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment())
            return;
        try
        {
            logger.LogDebug("Checking for Swagger changes...");

            foreach (var config in settings.OpenApiSettings.Apis.Where(x => x.WebClientOutputPath is not null))
            {
                try
                {
                    logger.LogDebug("Checking Swagger for {Name}", config.Name);
                    var swaggerJson = GetSwaggerJson(config);
                    if (!await HasSwaggerChanged(config, swaggerJson)) continue;
                    logger.LogInformation("Swagger changed detected for {Name}. Generating client...", config.Name);
                    var path = await SaveSwaggerHash(config, swaggerJson);
                    await GenerateTypeScriptClient(config, path);

                    logger.LogInformation("Client generated successfully for {Name}", config.Name);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing Swagger config {Name}", config.Name);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in Swagger client generator");
        }
    }

    private string GetSwaggerJson(OpenApiItem config)
    {
        var swaggerDoc = swaggerProvider.GetSwagger(config.Name);
        return swaggerDoc.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
    }

    private async Task<bool> HasSwaggerChanged(OpenApiItem config, string currentSwaggerJson)
    {
        var hashFilePath = GetHashFilePath(config);
        if (!File.Exists(hashFilePath))
            return true;

        var storedSwaggerJson = await File.ReadAllTextAsync(hashFilePath);
        return storedSwaggerJson != currentSwaggerJson;
    }

    private async Task<string> SaveSwaggerHash(OpenApiItem config, string content)
    {
        var hashFilePath = GetHashFilePath(config);

        var directory = Path.GetDirectoryName(hashFilePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        await File.WriteAllTextAsync(hashFilePath, content);
        return hashFilePath;
    }

    private async Task GenerateTypeScriptClient(OpenApiItem config, string swaggerJsonPath)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "npx",
            Arguments =
                $"swagger-typescript-api generate -p {swaggerJsonPath} -o {config.WebClientOutputPath} --unwrap-response-data --extract-enums --extract-responses --extract-request-body --extract-request-params --modular --module-name-first-tag --t ./templates",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = settings.SpaProxyServer.WorkingDirectory,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8
        };

        processStartInfo.Environment.Add("NODE_TLS_REJECT_UNAUTHORIZED", "0");

        using var process = new Process();
        process.StartInfo = processStartInfo;
        process.EnableRaisingEvents = true;
        process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                logger.LogInformation(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                logger.LogError(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();
    }

    private string GetHashFilePath(OpenApiItem config)
    {
        return Path.Combine(environment.ContentRootPath, "SwaggerFiles",
            $"swagger-{config.Name.ToLower()}.json");
    }
}