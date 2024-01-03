using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.PersistedGrant;

public class PersistedGrantDeletedEvent : AuditEvent
{
    public PersistedGrantDeletedEvent(string persistedGrantKey)
    {
        PersistedGrantKey = persistedGrantKey;
    }

    public string PersistedGrantKey { get; set; }
}