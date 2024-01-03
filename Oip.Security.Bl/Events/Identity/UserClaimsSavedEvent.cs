using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Identity;

public class UserClaimsSavedEvent<TUserClaimsDto> : AuditEvent
{
    public UserClaimsSavedEvent(TUserClaimsDto claims)
    {
        Claims = claims;
    }

    public TUserClaimsDto Claims { get; set; }
}