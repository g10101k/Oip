using System;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Client;

public class ClientSecretAddedEvent : AuditEvent
{
    public ClientSecretAddedEvent(int clientId, string type, DateTime? expiration)
    {
        ClientId = clientId;
        Type = type;
        Expiration = expiration;
    }

    public string Type { get; set; }

    public DateTime? Expiration { get; set; }

    public int ClientId { get; set; }
}