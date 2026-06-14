using Oip.Base.Runtime;
using Oip.Notifications.Base.Data.Entities;
using Oip.Notifications.Base.Data.Repositories;
using Oip.Notifications.Base.Services;

namespace Oip.Notifications.Base.Startups;

/// <summary>
/// Initializes notification channels by creating or skipping entries in the repository based on the available channels
/// in the service
/// </summary>
public class ChannelStartup(NotificationChannelRepository channelRepository, ChannelService channelService)
    : IStartupTask
{
    /// <inheritdoc />
    public int Order => 0;

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        foreach (var channel in channelService.Channels)
        {
            await channelRepository.CreateOrSkip(new NotificationChannelEntity()
            {
                Code =  channel.Code,
                Name = channel.Name,
                IsActive = true,
                MaxRetryCount = channel.MaxRetryCount,
                RequiresVerification = channel.RequiresVerification
            });
        }
    }
}