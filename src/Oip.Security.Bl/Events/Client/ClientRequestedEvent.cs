using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Client;

public class ClientRequestedEvent : AuditEvent
{
    public ClientRequestedEvent(ClientDto clientDto)
    {
        ClientDto = clientDto;
    }

    public ClientDto ClientDto { get; set; }
}