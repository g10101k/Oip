using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Client;

public class ClientClonedEvent : AuditEvent
{
    public ClientClonedEvent(ClientCloneDto client)
    {
        Client = client;
    }

    public ClientCloneDto Client { get; set; }
}