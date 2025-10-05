using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Oip.Base.Discovery;

public sealed class ServiceHealthChecker : IServiceHealthChecker, IDisposable
{
    private readonly IServiceRegistry _serviceRegistry;
    private readonly ILogger<ServiceHealthChecker> _logger;
    private readonly ServiceDiscoverySettings _settings;
    private readonly HttpClient _httpClient;
    private readonly Timer _healthCheckTimer;
    private bool _isChecking;

    public ServiceHealthChecker(
        IServiceRegistry serviceRegistry,
        IOptions<ServiceDiscoverySettings> options,
        ILogger<ServiceHealthChecker> logger)
    {
        _serviceRegistry = serviceRegistry;
        _settings = options.Value;
        _logger = logger;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        _healthCheckTimer = new Timer(CheckAllServicesHealth, null, Timeout.Infinite, Timeout.Infinite);
    }

    public Task StartHealthCheckingAsync()
    {
        _isChecking = true;
        _healthCheckTimer.Change(
            TimeSpan.FromSeconds(_settings.HealthCheckIntervalSeconds),
            TimeSpan.FromSeconds(_settings.HealthCheckIntervalSeconds));

        _logger.LogInformation("Service health checker started");
        return Task.CompletedTask;
    }

    public Task StopHealthCheckingAsync()
    {
        _isChecking = false;
        _healthCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);

        _logger.LogInformation("Service health checker stopped");
        return Task.CompletedTask;
    }

    public async Task<bool> CheckServiceHealthAsync(ServiceInfo service)
    {
        if (service.IsManual) return true; // Manual services are always considered healthy

        try
        {
            // Try HTTP health check first
            if (service.Protocols.Contains(Protocol.http) && !string.IsNullOrEmpty(service.HealthCheckUrl))
            {
                var response = await _httpClient.GetAsync(service.HealthCheckUrl);
                return response.IsSuccessStatusCode;
            }

            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(service.Host, service.Port);
            return tcpClient.Connected;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed for service {ServiceName}", service.ServiceName);
            return false;
        }
    }

    private async void CheckAllServicesHealth(object? state)
    {
        if (!_isChecking) return;

        try
        {
            var services = await _serviceRegistry.GetActiveServicesAsync();
            var tasks = services.Where(s => !s.IsManual).Select(CheckAndUpdateServiceHealthAsync);
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check cycle");
        }
    }

    private async Task CheckAndUpdateServiceHealthAsync(ServiceInfo service)
    {
        var isHealthy = await CheckServiceHealthAsync(service);
        if (!isHealthy)
        {
            _logger.LogWarning("Service {ServiceName} is unhealthy, unregistering", service.ServiceName);
            await _serviceRegistry.UnregisterServiceAsync(service.ServiceId);
        }
    }

    public void Dispose()
    {
        StopHealthCheckingAsync().GetAwaiter().GetResult();
        _healthCheckTimer?.Dispose();
        _httpClient?.Dispose();
    }
}