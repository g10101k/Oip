using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Client;

public class ClientPropertyAddedEvent : AuditEvent
{
    public ClientPropertyAddedEvent(ClientPropertiesDto clientProperties)
    {
        ClientProperties = clientProperties;
    }

    public ClientPropertiesDto ClientProperties { get; set; }
}