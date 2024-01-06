using System;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.ApiResource;

public class ApiSecretRequestedEvent : AuditEvent
{
    public ApiSecretRequestedEvent(int apiResourceId, int apiSecretId, string type, DateTime? expiration)
    {
        ApiResourceId = apiResourceId;
        ApiSecretId = apiSecretId;
        Type = type;
        Expiration = expiration;
    }

    public int ApiResourceId { get; set; }

    public int ApiSecretId { get; set; }

    public string Type { get; set; }

    public DateTime? Expiration { get; set; }
}