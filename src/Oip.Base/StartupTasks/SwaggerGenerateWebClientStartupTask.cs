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
public class SwaggerGenerateWebClientStartupTask(
    ISwaggerProvider swaggerProvider,
    IWebHostEnvironment environment,
    ILogger<SwaggerGenerateWebClientStartupTask> logger,
    IHostApplicationLifetime lifetime,
    IBaseOipModuleAppSettings settings) : IStartupTask
{
    /// <inheritdoc />
    public int Order => 0;

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Generating Swagger clients...");

            foreach (var config in settings.OpenApi.Where(x => x.GenerateCommand is not null))
            {
                string? path = null;
                try
                {
                    logger.LogDebug("Generating Swagger for {Name}", config.Name);
                    var swaggerJson = GetSwaggerJson(config);
                    logger.LogInformation("Generating client for {Name}...", config.Name);
                    path = await SaveSwaggerFile(config, swaggerJson);
                    await GenerateTypeScriptClient(config, path, cancellationToken);

                    logger.LogInformation("Client generated successfully for {Name}", config.Name);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing Swagger config {Name}", config.Name);
                    throw;
                }
                finally
                {
                    DeleteTempSwaggerFile(path);
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
            lifetime.StopApplication();
        }
    }

    private string GetSwaggerJson(OpenApiItem config)
    {
        var swaggerDoc = swaggerProvider.GetSwagger(config.Name);
        return swaggerDoc.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
    }

    private async Task<string> SaveSwaggerFile(OpenApiItem config, string content)
    {
        var tempFilePath = GetTempSwaggerFilePath(config);

        var directory = Path.GetDirectoryName(tempFilePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        await File.WriteAllTextAsync(tempFilePath, content);
        return tempFilePath;
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

    private string GetTempSwaggerFilePath(OpenApiItem config)
    {
        return Path.Combine(environment.ContentRootPath, "obj/SwaggerFiles",
            $"swagger-{config.Name.ToLower()}.json");
    }

    private static void DeleteTempSwaggerFile(string? tempFilePath)
    {
        if (tempFilePath is null || !File.Exists(tempFilePath))
            return;

        File.Delete(tempFilePath);
    }
}