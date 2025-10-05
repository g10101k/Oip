namespace Oip.Base.Discovery;

// Interfaces/IServiceRegistry.cs
public interface IServiceRegistry
{
    Task RegisterServiceAsync(ServiceInfo service);
    Task UnregisterServiceAsync(string serviceId);
    Task UpdateHeartbeatAsync(string serviceId);
    Task<IReadOnlyList<ServiceInfo>> GetActiveServicesAsync();
    Task<IReadOnlyList<ServiceInfo>> GetServicesByTypeAsync(string serviceType);
    Task<ServiceInfo?> GetServiceAsync(string serviceId);
    Task AddManualServiceAsync(ServiceInfo service);
    Task RemoveManualServiceAsync(string serviceId);
}

// Interfaces/IServiceBroadcaster.cs
public interface IServiceBroadcaster
{
    Task BroadcastRegistrationAsync();
    Task BroadcastHeartbeatAsync();
    Task BroadcastUnregistrationAsync();
    Task StartBroadcastingAsync();
    Task StopBroadcastingAsync();
}

// Interfaces/IServiceListener.cs
public interface IServiceListener
{
    Task StartListeningAsync();
    Task StopListeningAsync();
    event Func<ServiceInfo, Task> ServiceRegistered;
    event Func<ServiceInfo, Task> ServiceUnregistered;
}

// Interfaces/IServiceHealthChecker.cs
public interface IServiceHealthChecker
{
    Task StartHealthCheckingAsync();
    Task StopHealthCheckingAsync();
    Task<bool> CheckServiceHealthAsync(ServiceInfo service);
}