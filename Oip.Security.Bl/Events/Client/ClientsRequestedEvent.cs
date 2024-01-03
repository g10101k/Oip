using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Client;

public class ClientsRequestedEvent : AuditEvent
{
    public ClientsRequestedEvent(ClientsDto clientsDto)
    {
        ClientsDto = clientsDto;
    }

    public ClientsDto ClientsDto { get; set; }
}