using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.ApiResource;

public class ApiResourcesRequestedEvent : AuditEvent
{
    public ApiResourcesRequestedEvent(ApiResourcesDto apiResources)
    {
        ApiResources = apiResources;
    }

    public ApiResourcesDto ApiResources { get; set; }
}