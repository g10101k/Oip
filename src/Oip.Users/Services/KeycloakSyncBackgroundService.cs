using Oip.Base.Services;

namespace Oip.Users.Services;

/// <summary>
/// Background service for synchronizing users periodically.
/// </summary>
public class KeycloakSyncBackgroundService(
    ILogger<UserSyncService> logger,
    IServiceScopeFactory scopeFactory)
    : PeriodicBackgroundService<UserSyncService>(scopeFactory, logger);