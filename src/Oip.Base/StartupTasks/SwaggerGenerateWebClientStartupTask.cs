using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Oip.Base.Runtime;
using Oip.Base.Settings;
using Swashbuckle.AspNetCore.Swagger;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
    public int Order => 0;

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment())
            return;
        try
        {
            logger.LogDebug("Checking for Swagger changes...");

            foreach (var config in settings.OpenApi.Where(x => x.GenerateCommand is not null))
            {
                string? tempPath = null;
                try
                {
                    logger.LogDebug("Checking Swagger for {Name}", config.Name);
                    var swaggerJson = GetSwaggerJson(config);
                    if (!config.ForceGeneration)
                        if (!await HasSwaggerChanged(config, swaggerJson))
                            continue;
                    logger.LogInformation("Swagger changed detected for {Name}. Generating client...", config.Name);
                    tempPath = await SaveTempSwaggerFile(config, swaggerJson);
                    await GenerateTypeScriptClient(config, tempPath, cancellationToken);
                    await CommitSwaggerFile(config, tempPath);

                    logger.LogInformation("Client generated successfully for {Name}", config.Name);
                }
                catch (Exception ex)
                {
                    DeleteTempSwaggerFile(tempPath);
                    logger.LogError(ex, "Error processing Swagger config {Name}", config.Name);
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in Swagger client generator");
            throw;
        }
        finally
        {
            logger.LogInformation("Swagger generation complete");
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

    private async Task<string> SaveTempSwaggerFile(OpenApiItem config, string content)
    {
        var tempFilePath = GetTempSwaggerFilePath(config);

        var directory = Path.GetDirectoryName(tempFilePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        await File.WriteAllTextAsync(tempFilePath, content);
        return tempFilePath;
    }

    private Task CommitSwaggerFile(OpenApiItem config, string tempFilePath)
    {
        var hashFilePath = GetHashFilePath(config);

        var directory = Path.GetDirectoryName(hashFilePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        File.Move(tempFilePath, hashFilePath, true);
        return Task.CompletedTask;
    }

    protected virtual async Task GenerateTypeScriptClient(
        OpenApiItem config,
        string swaggerJsonPath,
        CancellationToken cancellationToken)
    {
        // In case of paths with spaces
        var protectedSwaggerJsonPath =
            swaggerJsonPath.Any(char.IsWhiteSpace) ? "\"" + swaggerJsonPath + "\"" : swaggerJsonPath;
        var generateCommand = config.GenerateCommand!;

        // Windows platform requires CMD to execute NPX command
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            generateCommand = "cmd.exe /c " + generateCommand;

        var parts = generateCommand.Replace("{SwaggerJsonPath}", protectedSwaggerJsonPath).Split(' ', 2);
        var arguments = parts.Length > 1 ? parts[1] : string.Empty;
        var workingDirectory = config.WorkingDirectory != null
            ? Path.GetFullPath(config.WorkingDirectory)
            : settings.SpaProxyServer.WorkingDirectory;

        var processStartInfo = new ProcessStartInfo
        {
            FileName = parts[0],
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8,
            Environment =
            {
                ["NODE_TLS_REJECT_UNAUTHORIZED"] = "0"
            }
        };

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
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Web API client generation failed. Command: '{processStartInfo.FileName} {processStartInfo.Arguments}', WorkingDirectory: '{workingDirectory}', ExitCode: {process.ExitCode}.");
        }
    }

    private string GetHashFilePath(OpenApiItem config)
    {
        return Path.Combine(environment.ContentRootPath, "obj/SwaggerFiles",
            $"swagger-{config.Name.ToLower()}.json");
    }

    private string GetTempSwaggerFilePath(OpenApiItem config)
    {
        return Path.Combine(environment.ContentRootPath, "obj/SwaggerFiles/tmp",
            $"swagger-{config.Name.ToLower()}.json");
    }

    private static void DeleteTempSwaggerFile(string? tempFilePath)
    {
        if (tempFilePath is null || !File.Exists(tempFilePath))
            return;

        File.Delete(tempFilePath);
    }
}