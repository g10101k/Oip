using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.ApiResource;

public class ApiResourcePropertyDeletedEvent : AuditEvent
{
    public ApiResourcePropertyDeletedEvent(ApiResourcePropertiesDto apiResourceProperty)
    {
        ApiResourceProperty = apiResourceProperty;
    }

    public ApiResourcePropertiesDto ApiResourceProperty { get; set; }
}