using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Identity;

public class RoleClaimRequestedEvent<TRoleClaimsDto> : AuditEvent
{
    public RoleClaimRequestedEvent(TRoleClaimsDto roleClaim)
    {
        RoleClaim = roleClaim;
    }

    public TRoleClaimsDto RoleClaim { get; set; }
}