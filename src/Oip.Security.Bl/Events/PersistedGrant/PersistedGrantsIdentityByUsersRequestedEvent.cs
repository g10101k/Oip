using Oip.Security.Bl.Dtos.Grant;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.PersistedGrant;

public class PersistedGrantsIdentityByUsersRequestedEvent : AuditEvent
{
    public PersistedGrantsIdentityByUsersRequestedEvent(PersistedGrantsDto persistedGrants)
    {
        PersistedGrants = persistedGrants;
    }

    public PersistedGrantsDto PersistedGrants { get; set; }
}