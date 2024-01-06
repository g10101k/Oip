using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Client;

public class ClientDeletedEvent : AuditEvent
{
    public ClientDeletedEvent(ClientDto client)
    {
        Client = client;
    }

    public ClientDto Client { get; set; }
}