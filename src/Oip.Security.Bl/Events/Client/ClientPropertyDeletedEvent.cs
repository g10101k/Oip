using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Client;

public class ClientPropertyDeletedEvent : AuditEvent
{
    public ClientPropertyDeletedEvent(ClientPropertiesDto clientProperty)
    {
        ClientProperty = clientProperty;
    }

    public ClientPropertiesDto ClientProperty { get; set; }
}