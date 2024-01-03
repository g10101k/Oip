using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.ApiScope;

public class ApiScopeRequestedEvent : AuditEvent
{
    public ApiScopeRequestedEvent(ApiScopeDto apiScopes)
    {
        ApiScopes = apiScopes;
    }

    public ApiScopeDto ApiScopes { get; set; }
}