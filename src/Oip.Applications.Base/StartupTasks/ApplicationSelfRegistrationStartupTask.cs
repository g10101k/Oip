using Microsoft.Extensions.Logging;
using Oip.Applications.Base.Contracts;
using Oip.Base.Runtime;
using Oip.Base.Settings;

namespace Oip.Applications.Base.StartupTasks;

/// <summary>
/// Registers the current service application passport at startup.
/// </summary>
public class ApplicationSelfRegistrationStartupTask(
    IApplicationRegistryService registryService,
    IBaseOipModuleAppSettings appSettings,
    ILogger<ApplicationSelfRegistrationStartupTask> logger)
    : IStartupTask
{
    /// <inheritdoc />
    public int Order => 10;

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(appSettings.Application.Code))
        {
            logger.LogDebug("Application self-registration skipped because Application:Code is empty.");
            return;
        }

        try
        {
            await registryService.RegisterApplicationAsync(new ApplicationRegistryItemDto
            {
                Code = appSettings.Application.Code,
                DisplayName = appSettings.Application.DisplayName,
                BaseUrl = appSettings.Application.BaseUrl,
                ApiBaseUrl = appSettings.Application.ApiBaseUrl,
                Icon = appSettings.Application.Icon,
                Order = appSettings.Application.Order,
                Enabled = appSettings.Application.Enabled,
                ServiceType = appSettings.Application.ServiceType
            }, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Application self-registration failed for {ApplicationCode}. Startup will continue.",
                appSettings.Application.Code);
        }
    }
}
