using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.ApiScope;

public class ApiScopePropertyRequestedEvent : AuditEvent
{
    public ApiScopePropertyRequestedEvent(int apiScopePropertyId, ApiScopePropertiesDto apiScopeProperty)
    {
        ApiScopePropertyId = apiScopePropertyId;
        ApiScopeProperty = apiScopeProperty;
    }

    public int ApiScopePropertyId { get; set; }

    public ApiScopePropertiesDto ApiScopeProperty { get; set; }
}