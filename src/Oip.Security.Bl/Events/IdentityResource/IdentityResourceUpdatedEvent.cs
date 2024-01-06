using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.IdentityResource;

public class IdentityResourceUpdatedEvent : AuditEvent
{
    public IdentityResourceUpdatedEvent(IdentityResourceDto originalIdentityResource,
        IdentityResourceDto identityResource)
    {
        OriginalIdentityResource = originalIdentityResource;
        IdentityResource = identityResource;
    }

    public IdentityResourceDto OriginalIdentityResource { get; set; }
    public IdentityResourceDto IdentityResource { get; set; }
}