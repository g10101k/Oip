using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.IdentityResource;

public class IdentityResourcesRequestedEvent : AuditEvent
{
    public IdentityResourcesRequestedEvent(IdentityResourcesDto identityResources)
    {
        IdentityResources = identityResources;
    }

    public IdentityResourcesDto IdentityResources { get; }
}