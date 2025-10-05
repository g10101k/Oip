using System.Runtime.Serialization;
using ProtoBuf;

namespace Oip.Base.Discovery;

public sealed class ServiceInfo
{
    public string ServiceId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public Protocol[] Protocols { get; set; } = [];
    public string HealthCheckUrl { get; set; } = string.Empty;
    public DateTime LastHeartbeat { get; set; }
    public bool IsManual { get; set; }
    public string Version { get; set; } = "1.0.0";
}

[ProtoContract]
public sealed class DiscoveryMessage
{
    [ProtoMember(1)] public DiscoveryMessageType MessageType { get; set; }
    [ProtoMember(2)] public string ServiceId { get; set; } = string.Empty;
    [ProtoMember(3)] public string ServiceName { get; set; } = string.Empty;
    [ProtoMember(4)] public string ServiceType { get; set; } = string.Empty;
    [ProtoMember(5)] public string Host { get; set; } = string.Empty;
    [ProtoMember(6)] public int Port { get; set; }
    [ProtoMember(7)] public Protocol[] Protocols { get; set; } = [];
    [ProtoMember(8)] public string HealthCheckUrl { get; set; } = string.Empty;
    [ProtoMember(9)] public long Timestamp { get; set; }
    [ProtoMember(10)] public string Signature { get; set; } = string.Empty;
    [ProtoMember(11)] public string Version { get; set; } = "1.0.0";
}

/// <summary>
/// Represents the options for service discovery.
/// </summary>
public sealed class ServiceDiscoverySettings
{
    public int BroadcastPort { get; set; } = 9876;
    public int ListenPort { get; set; } = 9876;
    public int HeartbeatIntervalSeconds { get; set; } = 30;
    public int HealthCheckIntervalSeconds { get; set; } = 60;
    public int ServiceTimeoutSeconds { get; set; } = 90;
    public string SecretKey { get; set; } = "default-secret-key";
    public bool EnableBroadcast { get; set; } = true;

    public ServiceInfo[] ManualServices { get; set; } = [];
}

public enum DiscoveryMessageType
{
    Register,
    Heartbeat,
    Unregister
}

public enum Protocol
{
    http,
    grpc
}