using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Identity;

public class RolesRequestedEvent<TRolesDto> : AuditEvent
{
    public RolesRequestedEvent(TRolesDto roles)
    {
        Roles = roles;
    }

    public TRolesDto Roles { get; set; }
}