using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Identity;

public class UserClaimRequestedEvent<TUserClaimsDto> : AuditEvent
{
    public UserClaimRequestedEvent(TUserClaimsDto userClaims)
    {
        UserClaims = userClaims;
    }

    public TUserClaimsDto UserClaims { get; set; }
}