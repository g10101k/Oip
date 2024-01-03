using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Client;

public class ClientPropertyRequestedEvent : AuditEvent
{
    public ClientPropertyRequestedEvent(ClientPropertiesDto clientProperties)
    {
        ClientProperties = clientProperties;
    }

    public ClientPropertiesDto ClientProperties { get; set; }
}