using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Identity;

public class UserUpdatedEvent<TUserDto> : AuditEvent
{
    public UserUpdatedEvent(TUserDto originalUser, TUserDto user)
    {
        OriginalUser = originalUser;
        User = user;
    }

    public TUserDto OriginalUser { get; set; }
    public TUserDto User { get; set; }
}