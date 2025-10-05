using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oip.Base.Settings;
using ProtoBuf;

namespace Oip.Base.Discovery;

public sealed class ServiceBroadcaster : IServiceBroadcaster, IDisposable
{
    private readonly ServiceDiscoverySettings _settings;
    private readonly ILogger<ServiceBroadcaster> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Timer _heartbeatTimer;
    private UdpClient? _udpClient;
    private bool _isBroadcasting;
    private ServiceInfo? _currentService;

    public ServiceBroadcaster(
        IBaseOipModuleAppSettings settings,
        ILogger<ServiceBroadcaster> logger,
        IServiceProvider serviceProvider)
    {
        _settings = settings.ServiceDiscovery;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _heartbeatTimer = new Timer(SendHeartbeat, null, Timeout.Infinite, Timeout.Infinite);
    }

    public async Task BroadcastRegistrationAsync()
    {
        var service = await GetCurrentServiceInfoAsync();
        if (service == null) return;

        _currentService = service;
        var message = CreateDiscoveryMessage(DiscoveryMessageType.Register, service);
        await BroadcastMessageAsync(message);
    }

    public async Task BroadcastHeartbeatAsync()
    {
        if (_currentService == null) return;

        var message = CreateDiscoveryMessage(DiscoveryMessageType.Heartbeat, _currentService);
        await BroadcastMessageAsync(message);
    }

    public async Task BroadcastUnregistrationAsync()
    {
        if (_currentService == null) return;

        var message = CreateDiscoveryMessage(DiscoveryMessageType.Unregister, _currentService);
        await BroadcastMessageAsync(message);
    }

    public async Task StartBroadcastingAsync()
    {
        if (!_settings.EnableBroadcast) return;

        try
        {
            _udpClient = new UdpClient();
            _udpClient.JoinMulticastGroup(await NetHelper.GetBroadcastAddressAsync());
            
            _udpClient.MulticastLoopback = true;
            _isBroadcasting = true;

            await BroadcastRegistrationAsync();

            _heartbeatTimer.Change(
                TimeSpan.FromSeconds(_settings.HeartbeatIntervalSeconds),
                TimeSpan.FromSeconds(_settings.HeartbeatIntervalSeconds));

            _logger.LogInformation("Service broadcasting started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start service broadcasting");
        }
    }

    public Task StopBroadcastingAsync()
    {
        _isBroadcasting = false;
        _heartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
        _udpClient?.Close();
        _udpClient?.Dispose();
        _udpClient = null;

        _logger.LogInformation("Service broadcasting stopped");
        return Task.CompletedTask;
    }

    private async void SendHeartbeat(object? state)
    {
        try
        {
            if (!_isBroadcasting) return;
            await BroadcastHeartbeatAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending heartbeat");
        }
    }

    private static byte[] SerializeToByteArray(DiscoveryMessage? message)
    {
        if (message == null)
        {
            return [];
        }

        using var stream = new MemoryStream();
        Serializer.Serialize(stream, message);
        return stream.ToArray();
    }

    private async Task BroadcastMessageAsync(DiscoveryMessage message)
    {
        if (_udpClient == null) return;

        try
        {
            _logger.LogInformation("Broadcasting message");

            var data = SerializeToByteArray(message);

            var broadcastAddress = await NetHelper.GetBroadcastAddressAsync();
            var endPoint = new IPEndPoint(broadcastAddress, _settings.BroadcastPort);

            await _udpClient.SendAsync(data, data.Length, endPoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting message");
        }
    }

    private DiscoveryMessage CreateDiscoveryMessage(DiscoveryMessageType messageType, ServiceInfo service)
    {
        var message = new DiscoveryMessage
        {
            MessageType = messageType,
            ServiceId = service.ServiceId,
            ServiceName = service.ServiceName,
            ServiceType = service.ServiceType,
            Host = service.Host,
            Port = service.Port,
            Protocols = service.Protocols,
            HealthCheckUrl = service.HealthCheckUrl,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Version = service.Version
        };

        message.Signature = CalculateSignature(message, _settings.SecretKey);
        return message;
    }

    public static string CalculateSignature(DiscoveryMessage message, string secretKey)
    {
        var signData = $"{message.ServiceId}:{message.Timestamp}:{secretKey}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(signData));
        return Convert.ToBase64String(hash);
    }

    

    private async Task<ServiceInfo?> GetCurrentServiceInfoAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var configuration = scope.ServiceProvider.GetService<IConfiguration>();

            var hostName = Dns.GetHostName();
            var ips = await Dns.GetHostAddressesAsync(hostName);
            var ip = ips.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "localhost";

            var serviceName = configuration?["ServiceDiscovery:ServiceName"] ?? "UnknownService";
            var serviceType = configuration?["ServiceDiscovery:ServiceType"] ?? "Generic";
            var port = int.Parse(configuration?["ASPNETCORE_HTTPS_PORT"] ??
                                 configuration?["ASPNETCORE_HTTP_PORT"] ?? "8080");

            return new ServiceInfo
            {
                ServiceId = $"{serviceName}_{hostName}_{port}",
                ServiceName = serviceName,
                ServiceType = serviceType,
                Host = ip,
                Port = port,
                Protocols = [Protocol.http],
                HealthCheckUrl = $"http://{ip}:{port}/health",
                Version = "1.0.0"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current service info");
            return null;
        }
    }

    public void Dispose()
    {
        StopBroadcastingAsync().GetAwaiter().GetResult();
        _heartbeatTimer?.Dispose();
        _udpClient?.Dispose();
    }
}