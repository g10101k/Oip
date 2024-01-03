using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.ApiScope;

public class ApiScopePropertyDeletedEvent : AuditEvent
{
    public ApiScopePropertyDeletedEvent(ApiScopePropertiesDto apiScopeProperty)
    {
        ApiScopeProperty = apiScopeProperty;
    }

    public ApiScopePropertiesDto ApiScopeProperty { get; set; }
}