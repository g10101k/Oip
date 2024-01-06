using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Identity;

public class RoleUsersRequestedEvent<TUsersDto> : AuditEvent
{
    public RoleUsersRequestedEvent(TUsersDto users)
    {
        Users = users;
    }

    public TUsersDto Users { get; set; }
}