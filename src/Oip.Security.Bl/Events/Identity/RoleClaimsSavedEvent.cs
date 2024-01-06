using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Identity;

public class RoleClaimsSavedEvent<TRoleClaimsDto> : AuditEvent
{
    public RoleClaimsSavedEvent(TRoleClaimsDto claims)
    {
        Claims = claims;
    }

    public TRoleClaimsDto Claims { get; set; }
}