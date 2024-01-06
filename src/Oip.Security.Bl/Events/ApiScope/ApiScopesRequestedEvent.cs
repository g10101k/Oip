using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.ApiScope;

public class ApiScopesRequestedEvent : AuditEvent
{
    public ApiScopesRequestedEvent(ApiScopesDto apiScope)
    {
        ApiScope = apiScope;
    }

    public ApiScopesDto ApiScope { get; set; }
}