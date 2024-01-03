using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.PersistedGrant;

public class PersistedGrantsDeletedEvent : AuditEvent
{
    public PersistedGrantsDeletedEvent(string userId)
    {
        UserId = userId;
    }

    public string UserId { get; set; }
}