using System.Collections.Generic;
using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Identity;

public class AllRolesRequestedEvent<TRoleDto> : AuditEvent
{
    public AllRolesRequestedEvent(List<TRoleDto> roles)
    {
        Roles = roles;
    }

    public List<TRoleDto> Roles { get; set; }
}