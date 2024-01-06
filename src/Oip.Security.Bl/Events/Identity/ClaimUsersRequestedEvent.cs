using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Identity;

public class ClaimUsersRequestedEvent<TUsersDto> : AuditEvent
{
    public ClaimUsersRequestedEvent(TUsersDto users)
    {
        Users = users;
    }

    public TUsersDto Users { get; set; }
}