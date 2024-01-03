using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Identity;

public class RoleRequestedEvent<TRoleDto> : AuditEvent
{
    public RoleRequestedEvent(TRoleDto role)
    {
        Role = role;
    }

    public TRoleDto Role { get; set; }
}