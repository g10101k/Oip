using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProtoBuf;

namespace Oip.Base.Discovery;

public sealed class ServiceListener(
    IOptions<ServiceDiscoverySettings> options,
    ILogger<ServiceListener> logger,
    IServiceRegistry serviceRegistry)
    : IServiceListener, IDisposable
{
    private readonly ServiceDiscoverySettings _settings = options.Value;
    private UdpClient? _udpClient;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isListening;

    public event Func<ServiceInfo, Task>? ServiceRegistered;
    public event Func<ServiceInfo, Task>? ServiceUnregistered;

    public async Task StartListeningAsync()
    {
        if (_isListening) return;

        try
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _udpClient = new UdpClient(_settings.ListenPort);
            var broadcastAddress = await NetHelper.GetBroadcastAddressAsync();

            _udpClient.JoinMulticastGroup(broadcastAddress);
            _udpClient.MulticastLoopback = true;

            _isListening = true;

            _ = Task.Run(() => ListenForMessagesAsync(_cancellationTokenSource.Token));

            logger.LogInformation("Service listener started on port {Port}", _settings.ListenPort);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start service listener");
        }
    }

    public Task StopListeningAsync()
    {
        _isListening = false;
        _cancellationTokenSource?.Cancel();
        _udpClient?.Close();
        _udpClient?.Dispose();
        _udpClient = null;

        logger.LogInformation("Service listener stopped");
        return Task.CompletedTask;
    }

    private static T? Deserialize<T>(byte[]? data) where T : class
    {
        if (data == null)
        {
            return null;
        }

        using var stream = new MemoryStream(data);
        return Serializer.Deserialize<T>(stream);
    }

    private async Task ListenForMessagesAsync(CancellationToken cancellationToken)
    {
        while (_isListening && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_udpClient == null) break;

                var result = await _udpClient.ReceiveAsync(cancellationToken);
                logger.LogInformation("Receive message");

                var message = Deserialize<DiscoveryMessage>(result.Buffer);

                if (message != null && ValidateMessage(message))
                {
                    await ProcessMessageAsync(message);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing discovery message");
            }
        }
    }

    private bool ValidateMessage(DiscoveryMessage message)
    {
        // Check timestamp for replay attacks (5 minutes window)
        var messageTime = DateTimeOffset.FromUnixTimeMilliseconds(message.Timestamp);
        if (DateTimeOffset.UtcNow - messageTime > TimeSpan.FromMinutes(5))
        {
            logger.LogWarning("Received expired discovery message from {ServiceId}", message.ServiceId);
            return false;
        }

        // Validate signature
        var expectedSignature = ServiceBroadcaster.CalculateSignature(message, _settings.SecretKey);
        if (!string.Equals(message.Signature, expectedSignature, StringComparison.Ordinal))
        {
            logger.LogWarning("Invalid signature in discovery message from {ServiceId}", message.ServiceId);
            return false;
        }

        return true;
    }

    private async Task ProcessMessageAsync(DiscoveryMessage message)
    {
        var serviceInfo = new ServiceInfo
        {
            ServiceId = message.ServiceId,
            ServiceName = message.ServiceName,
            ServiceType = message.ServiceType,
            Host = message.Host,
            Port = message.Port,
            Protocols = message.Protocols,
            HealthCheckUrl = message.HealthCheckUrl,
            LastHeartbeat = DateTime.UtcNow,
            Version = message.Version
        };

        switch (message.MessageType)
        {
            case DiscoveryMessageType.Register:
                await serviceRegistry.RegisterServiceAsync(serviceInfo);
                if (ServiceRegistered != null)
                {
                    await ServiceRegistered.Invoke(serviceInfo);
                }

                logger.LogInformation("Service registered via discovery: {ServiceName}", serviceInfo.ServiceName);
                break;

            case DiscoveryMessageType.Heartbeat:
                await serviceRegistry.UpdateHeartbeatAsync(serviceInfo.ServiceId);
                break;

            case DiscoveryMessageType.Unregister:
                await serviceRegistry.UnregisterServiceAsync(serviceInfo.ServiceId);
                if (ServiceUnregistered != null)
                {
                    await ServiceUnregistered.Invoke(serviceInfo);
                }

                logger.LogInformation("Service unregistered via discovery: {ServiceName}", serviceInfo.ServiceName);
                break;
        }
    }

    public void Dispose()
    {
        StopListeningAsync().GetAwaiter().GetResult();
        _cancellationTokenSource?.Dispose();
        _udpClient?.Dispose();
    }
}