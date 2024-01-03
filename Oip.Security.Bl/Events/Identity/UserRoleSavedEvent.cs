using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Identity;

public class UserRoleSavedEvent<TUserRolesDto> : AuditEvent
{
    public UserRoleSavedEvent(TUserRolesDto role)
    {
        Role = role;
    }

    public TUserRolesDto Role { get; set; }
}