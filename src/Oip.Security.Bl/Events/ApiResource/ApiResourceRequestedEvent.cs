using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.ApiResource;

public class ApiResourceRequestedEvent : AuditEvent
{
    public ApiResourceRequestedEvent(int apiResourceId, ApiResourceDto apiResource)
    {
        ApiResourceId = apiResourceId;
        ApiResource = apiResource;
    }

    public int ApiResourceId { get; set; }
    public ApiResourceDto ApiResource { get; set; }
}