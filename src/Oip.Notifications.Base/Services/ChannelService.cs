using System.Net.Mail;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oip.Base.Services;

namespace Oip.Notifications.Base.Services;

/// <summary>
/// Provides channel-based notification functionality by dynamically discovering and managing notification channel
/// implementations for sending messages with optional attachments
/// </summary>
public class ChannelService(IServiceProvider serviceProvider, ILogger<ChannelService> logger)
{
    private List<INotificationChannel>? _channels;

    /// <summary>
    /// Gets the list of notification channels registered in the service
    /// </summary>
    public List<INotificationChannel> Channels
    {
        get
        {
            if (_channels != null) return _channels;

            _channels = [];
            var assembly = Assembly.GetExecutingAssembly();

            var channelTypes = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } &&
                            typeof(INotificationChannel).IsAssignableFrom(t));
            var scope = serviceProvider.CreateScope();
            foreach (var type in channelTypes)
            {
                try
                {
                    if (ActivatorUtilities.CreateInstance(scope.ServiceProvider, type) is INotificationChannel channel)
                        _channels.Add(channel);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "An error occured while creating channel: {Name}", type.Name);
                }
            }

            return _channels;
        }
    }

    /// <summary>
    /// Notification via all channels with attachments
    /// </summary>
    /// <param name="channelCode">Channel</param>
    /// <param name="user">User to notify</param>
    /// <param name="subject">Message subject</param>
    /// <param name="message">Message content</param>
    /// <param name="importanceLevel">Importance level</param>
    /// <param name="attachments">File attachments</param>
    public bool Notify(string channelCode, UserCacheDto user, string subject, string message, ImportanceLevel importanceLevel,
        Attachment[]? attachments = null)
    {
        if (_channels == null) throw new InvalidOperationException("Channels is not initialized");
        var channel = _channels.FirstOrDefault(c => c.Code == channelCode);

        if (channel == null)
        {
            logger.LogError("Channel {ChannelCode} not found", channelCode);
            return false;
        }

        try
        {
            channel.ProcessNotify(new NotificationDto()
            {
                User = new UserInfoDto()
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    Phone = user.Phone ?? string.Empty,
                },
                Subject = subject,
                Message = message,
                ImportanceLevel = importanceLevel,
                Attachment = attachments ?? []
            });
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while notifying user {UserId} through channel {ChannelCode}",
                user.UserId, channelCode);
            return false;
        }
    }
}
