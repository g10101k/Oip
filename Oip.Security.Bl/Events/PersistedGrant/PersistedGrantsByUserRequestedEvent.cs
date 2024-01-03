using Oip.Security.Bl.Dtos.Grant;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.PersistedGrant;

public class PersistedGrantsByUserRequestedEvent : AuditEvent
{
    public PersistedGrantsByUserRequestedEvent(PersistedGrantsDto persistedGrants)
    {
        PersistedGrants = persistedGrants;
    }

    public PersistedGrantsDto PersistedGrants { get; set; }
}