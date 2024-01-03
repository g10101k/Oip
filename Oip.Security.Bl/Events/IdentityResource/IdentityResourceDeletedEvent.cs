using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.IdentityResource;

public class IdentityResourceDeletedEvent : AuditEvent
{
    public IdentityResourceDeletedEvent(IdentityResourceDto identityResource)
    {
        IdentityResource = identityResource;
    }

    public IdentityResourceDto IdentityResource { get; set; }
}