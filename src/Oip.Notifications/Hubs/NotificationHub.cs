using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Oip.Users.Base;

namespace Oip.Notifications.Hubs;

/// <summary>
/// SignalR hub for managing real-time notification connections.
/// </summary>
/// <param name="userCacheRepository">Repository for user cache operations.</param>
[Authorize]
public class NotificationHub(UserCacheRepository userCacheRepository) : Hub
{
    /// <inheritdoc />
    public override async Task OnConnectedAsync()
    {
        var userIdFromIdentityService = Context.UserIdentifier!;

        var userId = userCacheRepository.GetUserByKeycloakUserId(userIdFromIdentityService)?.UserId ??
                     throw new InvalidOperationException("Authentification failed");
        
        await Groups.AddToGroupAsync(Context.ConnectionId, $"{userId}");
        Context.Items["UserId"] = $"{userId}";
        await base.OnConnectedAsync();
    }
}