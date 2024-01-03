using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Identity;

public class RoleClaimsRequestedEvent<TRoleClaimsDto> : AuditEvent
{
    public RoleClaimsRequestedEvent(TRoleClaimsDto roleClaims)
    {
        RoleClaims = roleClaims;
    }

    public TRoleClaimsDto RoleClaims { get; set; }
}