using System;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Client;

public class ClientSecretRequestedEvent : AuditEvent
{
    public ClientSecretRequestedEvent(int clientId, int clientSecretId, string type, DateTime? expiration)
    {
        ClientId = clientId;
        ClientSecretId = clientSecretId;
        Type = type;
        Expiration = expiration;
    }

    public int ClientId { get; set; }

    public int ClientSecretId { get; set; }

    public string Type { get; set; }

    public DateTime? Expiration { get; set; }
}