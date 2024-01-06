using System;
using System.Collections.Generic;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Client;

public class ClientSecretsRequestedEvent : AuditEvent
{
    public ClientSecretsRequestedEvent(int clientId,
        List<(int clientSecretId, string type, DateTime? expiration)> secrets)
    {
        ClientId = clientId;
        Secrets = secrets;
    }

    public int ClientId { get; set; }

    public List<(int clientSecretId, string type, DateTime? expiration)> Secrets { get; set; }
}