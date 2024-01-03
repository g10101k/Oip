using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.ApiResource;

public class ApiResourceAddedEvent : AuditEvent
{
    public ApiResourceAddedEvent(ApiResourceDto apiResource)
    {
        ApiResource = apiResource;
    }

    public ApiResourceDto ApiResource { get; set; }
}