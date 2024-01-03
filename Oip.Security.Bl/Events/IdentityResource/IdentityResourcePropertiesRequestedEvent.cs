using Oip.Security.Bl.Dtos.Configuration;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.IdentityResource;

public class IdentityResourcePropertiesRequestedEvent : AuditEvent
{
    public IdentityResourcePropertiesRequestedEvent(IdentityResourcePropertiesDto identityResourceProperties)
    {
        IdentityResourceProperties = identityResourceProperties;
    }

    public IdentityResourcePropertiesDto IdentityResourceProperties { get; set; }
}