using Oip.Base.Security.DefaultSecrets;
using Oip.Base.Settings.Attributes;

namespace Oip.Hitl.Settings;

/// <summary>
/// OpenAI-compatible agent gateway used by chat UIs such as Open WebUI.
/// </summary>
public class AgentGatewaySettings
{
    /// <summary>
    /// Redis connection string of the streams the agent answers are passed through to the gateway; when empty, the
    /// connection string of the authentication ticket store is used.
    /// </summary>
    [SecretSetting(KnownDefaultSecrets.AuthTicketStoreRedisConnectionString, Required = false)]
    public string? RedisConnectionString { get; set; }

    /// <summary>
    /// Minutes the events of an agent run are kept in Redis.
    /// </summary>
    public int StreamTtlMinutes { get; set; } = 60;

    /// <summary>
    /// Maximum duration of an agent run in minutes, after which Temporal times the workflow out.
    /// </summary>
    public int RunTimeoutMinutes { get; set; } = 10;
}
