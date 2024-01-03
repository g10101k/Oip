using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.ApiScope;

public class ApiScopeUpdatedEvent : AuditEvent
{
    public ApiScopeUpdatedEvent(ApiScopeDto originalApiScope, ApiScopeDto apiScope)
    {
        OriginalApiScope = originalApiScope;
        ApiScope = apiScope;
    }

    public ApiScopeDto OriginalApiScope { get; set; }
    public ApiScopeDto ApiScope { get; set; }
}