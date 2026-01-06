using System.Net.Mail;
using System.Reflection;
using Oip.Notifications.Base;

namespace Oip.Notifications.Services;

/// <summary>
/// Сервис, который регистрирует каналы в бд
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
                if (ActivatorUtilities.CreateInstance(scope.ServiceProvider, type) is INotificationChannel channel)
                    _channels.Add(channel);
            }

            return _channels;
        }
    }


    /// <summary>
    /// Notification via all channels
    /// </summary>
    /// <param name="user">User to notify</param>
    /// <param name="subject">Message subject</param>
    /// <param name="message">Message content</param>
    public void Notify(UserInfoDto user, string subject, string message)
    {
        if (_channels == null) throw new InvalidOperationException("Channels is not initialized");

        foreach (var channel in _channels)
        {
            try
            {
                channel.Notify(user, subject, message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while sending notification via channel {channelName}",
                    channel.Name);
            }
        }
    }

    /// <summary>
    /// Notification via all channels with attachments
    /// </summary>
    /// <param name="user">User to notify</param>
    /// <param name="subject">Message subject</param>
    /// <param name="message">Message content</param>
    /// <param name="attachments">File attachments</param>
    public void Notify(UserInfoDto user, string subject, string message, Attachment[] attachments)
    {
        if (_channels == null) throw new InvalidOperationException("Channels is not initialized");
        
        foreach (var channel in _channels)
        {
            try
            {
                channel.Notify(user, subject, message, attachments);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while sending notification via channel {channelName}",
                    channel.Name);
            }
        }
    }
}