using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.ApiResource;

public class ApiResourcePropertyAddedEvent : AuditEvent
{
    public ApiResourcePropertyAddedEvent(ApiResourcePropertiesDto apiResourceProperty)
    {
        ApiResourceProperty = apiResourceProperty;
    }

    public ApiResourcePropertiesDto ApiResourceProperty { get; set; }
}