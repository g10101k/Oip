using System.Reflection;
using Oip.Notifications.Base;

namespace Oip.Notifications.Services;

/// <summary>
/// Сервис, который регистрирует каналы в бд
/// </summary>
public class ChannelService(IServiceProvider serviceProvider)
{
    private List<INotificationChannel>? _channels;

    /// <summary>
    /// Gets the list of notification channels registered in the service
    /// </summary>
    public List<INotificationChannel> Channels
    {
        get
        {
            if (_channels == null)
            {
                _channels = new List<INotificationChannel>();
                var assembly = Assembly.GetExecutingAssembly();

                var channelTypes = assembly.GetTypes()
                    .Where(t => t is { IsClass: true, IsAbstract: false } &&
                                typeof(INotificationChannel).IsAssignableFrom(t));

                foreach (var type in channelTypes)
                {
                    if (ActivatorUtilities.CreateInstance(serviceProvider, type) is INotificationChannel channel)
                        _channels.Add(channel);
                }
            }

            return _channels;
        }
    }
}