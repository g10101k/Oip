using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.ApiResource;

public class ApiResourceDeletedEvent : AuditEvent
{
    public ApiResourceDeletedEvent(ApiResourceDto apiResource)
    {
        ApiResource = apiResource;
    }

    public ApiResourceDto ApiResource { get; set; }
}