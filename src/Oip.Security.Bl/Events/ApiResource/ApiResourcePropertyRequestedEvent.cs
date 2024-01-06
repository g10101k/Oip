using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.ApiResource;

public class ApiResourcePropertyRequestedEvent : AuditEvent
{
    public ApiResourcePropertyRequestedEvent(int apiResourcePropertyId, ApiResourcePropertiesDto apiResourceProperties)
    {
        ApiResourcePropertyId = apiResourcePropertyId;
        ApiResourceProperties = apiResourceProperties;
    }

    public int ApiResourcePropertyId { get; }
    public ApiResourcePropertiesDto ApiResourceProperties { get; set; }
}