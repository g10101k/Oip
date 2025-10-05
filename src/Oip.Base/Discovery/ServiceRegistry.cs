using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Oip.Base.Discovery;

public sealed class ServiceRegistry : IServiceRegistry, IDisposable
{
    private readonly ConcurrentDictionary<string, ServiceInfo> _services = new();
    private readonly ConcurrentDictionary<string, ServiceInfo> _manualServices = new();
    private readonly ILogger<ServiceRegistry> _logger;
    private readonly ServiceDiscoverySettings _settings;
    private readonly Timer _cleanupTimer;

    public ServiceRegistry(IOptions<ServiceDiscoverySettings> options, ILogger<ServiceRegistry> logger)
    {
        _settings = options.Value;
        _logger = logger;
        _cleanupTimer = new Timer(CleanupExpiredServices, null, 
            TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public Task RegisterServiceAsync(ServiceInfo service)
    {
        service.LastHeartbeat = DateTime.UtcNow;
        _services.AddOrUpdate(service.ServiceId, service, (id, existing) => service);
        
        _logger.LogInformation("Service registered: {ServiceName} ({ServiceId}) at {Host}:{Port}", 
            service.ServiceName, service.ServiceId, service.Host, service.Port);
            
        return Task.CompletedTask;
    }

    public Task UnregisterServiceAsync(string serviceId)
    {
        if (_services.TryRemove(serviceId, out var service))
        {
            _logger.LogInformation("Service unregistered: {ServiceName} ({ServiceId})", 
                service.ServiceName, service.ServiceId);
        }
        return Task.CompletedTask;
    }

    public Task UpdateHeartbeatAsync(string serviceId)
    {
        if (_services.TryGetValue(serviceId, out var service))
        {
            service.LastHeartbeat = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ServiceInfo>> GetActiveServicesAsync()
    {
        var activeServices = _services.Values
            .Where(s => !IsServiceExpired(s))
            .Concat(_manualServices.Values)
            .ToList()
            .AsReadOnly();
            
        return Task.FromResult<IReadOnlyList<ServiceInfo>>(activeServices);
    }

    public Task<IReadOnlyList<ServiceInfo>> GetServicesByTypeAsync(string serviceType)
    {
        var services = _services.Values
            .Where(s => s.ServiceType.Equals(serviceType, StringComparison.OrdinalIgnoreCase) && !IsServiceExpired(s))
            .Concat(_manualServices.Values.Where(s => s.ServiceType.Equals(serviceType, StringComparison.OrdinalIgnoreCase)))
            .ToList()
            .AsReadOnly();
            
        return Task.FromResult<IReadOnlyList<ServiceInfo>>(services);
    }

    public Task<ServiceInfo?> GetServiceAsync(string serviceId)
    {
        if (_services.TryGetValue(serviceId, out var service) && !IsServiceExpired(service))
        {
            return Task.FromResult<ServiceInfo?>(service);
        }
        
        if (_manualServices.TryGetValue(serviceId, out var manualService))
        {
            return Task.FromResult<ServiceInfo?>(manualService);
        }
        
        return Task.FromResult<ServiceInfo?>(null);
    }

    public Task AddManualServiceAsync(ServiceInfo service)
    {
        service.IsManual = true;
        _manualServices.AddOrUpdate(service.ServiceId, service, (id, existing) => service);
        
        _logger.LogInformation("Manual service added: {ServiceName} ({ServiceId})", 
            service.ServiceName, service.ServiceId);
            
        return Task.CompletedTask;
    }

    public Task RemoveManualServiceAsync(string serviceId)
    {
        if (_manualServices.TryRemove(serviceId, out var service))
        {
            _logger.LogInformation("Manual service removed: {ServiceName} ({ServiceId})", 
                service.ServiceName, service.ServiceId);
        }
        return Task.CompletedTask;
    }

    private bool IsServiceExpired(ServiceInfo service)
    {
        if (service.IsManual) return false;
        
        return DateTime.UtcNow - service.LastHeartbeat > TimeSpan.FromSeconds(_settings.ServiceTimeoutSeconds);
    }

    private void CleanupExpiredServices(object? state)
    {
        try
        {
            var expiredServices = _services.Where(kvp => IsServiceExpired(kvp.Value)).ToList();
            
            foreach (var (serviceId, service) in expiredServices)
            {
                if (_services.TryRemove(serviceId, out _))
                {
                    _logger.LogWarning("Service expired and removed: {ServiceName} ({ServiceId})", 
                        service.ServiceName, service.ServiceId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup of expired services");
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}