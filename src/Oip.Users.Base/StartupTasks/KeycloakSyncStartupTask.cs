using Microsoft.Extensions.Logging;
using Oip.Base.Runtime;
using Oip.Users.Base.Services;

namespace Oip.Users.Base.StartupTasks;

/// <summary>
/// Startup task for synchronizing users from Keycloak once when the application starts.
/// </summary>
public class KeycloakSyncStartupTask(KeycloakSyncService keycloakSyncService, ILogger<KeycloakSyncStartupTask> logger)
    : IStartupTask
{
    /// <inheritdoc />
    public int Order => 20;

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting Keycloak user synchronization startup task.");
            await keycloakSyncService.SyncAllUsersAsync(cancellationToken);
            logger.LogInformation("Keycloak user synchronization startup task completed.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Keycloak user synchronization task cancelled.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Keycloak user synchronization startup task failed.");
            throw;
        }
    }
}