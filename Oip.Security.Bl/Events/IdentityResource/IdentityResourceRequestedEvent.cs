using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.IdentityResource;

public class IdentityResourceRequestedEvent : AuditEvent
{
    public IdentityResourceRequestedEvent(IdentityResourceDto identityResource)
    {
        IdentityResource = identityResource;
    }

    public IdentityResourceDto IdentityResource { get; set; }
}