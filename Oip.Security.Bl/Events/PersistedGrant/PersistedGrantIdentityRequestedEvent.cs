using Oip.Security.Bl.Dtos.Grant;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.PersistedGrant;

public class PersistedGrantIdentityRequestedEvent : AuditEvent
{
    public PersistedGrantIdentityRequestedEvent(PersistedGrantDto persistedGrant)
    {
        PersistedGrant = persistedGrant;
    }

    public PersistedGrantDto PersistedGrant { get; set; }
}