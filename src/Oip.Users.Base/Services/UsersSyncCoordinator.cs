using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oip.Users.Base.Notifications;

namespace Oip.Users.Base.Services;

/// <summary>
/// Runs the full synchronization of users with Keycloak in the background, one run at a time,
/// and keeps the state of the last run.
/// </summary>
public class UsersSyncCoordinator(IServiceScopeFactory scopeFactory, ILogger<UsersSyncCoordinator> logger)
{
    private readonly Lock _lock = new();
    private UsersSyncStatus _status = new();

    /// <summary>
    /// Gets the state of the current or the last synchronization.
    /// </summary>
    public UsersSyncStatus Status
    {
        get
        {
            lock (_lock)
                return _status;
        }
    }

    /// <summary>
    /// Starts the full synchronization in the background unless another one is still running.
    /// </summary>
    /// <param name="status">The state of the started synchronization, or of the running one.</param>
    /// <returns><c>true</c> when the synchronization was started; <c>false</c> when one is already running.</returns>
    public bool TryStart(out UsersSyncStatus status)
    {
        lock (_lock)
        {
            if (_status.IsRunning)
            {
                status = _status;
                return false;
            }

            _status = new UsersSyncStatus { IsRunning = true, StartedAt = DateTimeOffset.UtcNow };
            status = _status;
        }

        _ = Task.Run(RunAsync);
        return true;
    }

    /// <summary>
    /// Synchronizes all users with Keycloak.
    /// </summary>
    /// <param name="services">The services of the scope created for the run.</param>
    /// <returns>The summary of the completed synchronization.</returns>
    protected virtual Task<SyncUsersCompleteNotify> SyncAllUsersAsync(IServiceProvider services)
    {
        return services.GetRequiredService<KeycloakSyncService>().SyncAllUsersAsync();
    }

    private async Task RunAsync()
    {
        UsersSyncStatus finished;
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var result = await SyncAllUsersAsync(scope.ServiceProvider);
            finished = new UsersSyncStatus
            {
                StartedAt = result.Start,
                FinishedAt = result.End,
                SyncedCount = result.Count
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error syncing all users in background");
            finished = new UsersSyncStatus
            {
                StartedAt = Status.StartedAt,
                FinishedAt = DateTimeOffset.UtcNow,
                Failed = true
            };
        }

        lock (_lock)
            _status = finished;
    }
}

/// <summary>
/// State of the full synchronization of users with Keycloak.
/// </summary>
public record UsersSyncStatus
{
    /// <summary>
    /// Whether the synchronization is running.
    /// </summary>
    public bool IsRunning { get; init; }

    /// <summary>
    /// When the current or the last synchronization started; <c>null</c> when none was started.
    /// </summary>
    public DateTimeOffset? StartedAt { get; init; }

    /// <summary>
    /// When the last synchronization finished; <c>null</c> while it runs or when none was started.
    /// </summary>
    public DateTimeOffset? FinishedAt { get; init; }

    /// <summary>
    /// Number of users the last synchronization processed; <c>null</c> when it has not finished successfully.
    /// </summary>
    public int? SyncedCount { get; init; }

    /// <summary>
    /// Whether the last synchronization failed.
    /// </summary>
    public bool Failed { get; init; }
}
