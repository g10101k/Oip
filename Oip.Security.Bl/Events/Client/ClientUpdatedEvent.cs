using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Client;

public class ClientUpdatedEvent : AuditEvent
{
    public ClientUpdatedEvent(ClientDto originalClient, ClientDto client)
    {
        OriginalClient = originalClient;
        Client = client;
    }

    public ClientDto OriginalClient { get; set; }
    public ClientDto Client { get; set; }
}