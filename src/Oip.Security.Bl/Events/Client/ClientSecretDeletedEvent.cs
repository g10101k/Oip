using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Client;

public class ClientSecretDeletedEvent : AuditEvent
{
    public ClientSecretDeletedEvent(int clientId, int clientSecretId)
    {
        ClientId = clientId;
        ClientSecretId = clientSecretId;
    }

    public int ClientId { get; set; }

    public int ClientSecretId { get; set; }
}