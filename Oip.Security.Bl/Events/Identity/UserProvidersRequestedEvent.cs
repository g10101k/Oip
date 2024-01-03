using Skoruba.AuditLogging.Events;

namespace Oip.Security.Bl.Events.Identity;

public class UserProvidersRequestedEvent<TUserProvidersDto> : AuditEvent
{
    public UserProvidersRequestedEvent(TUserProvidersDto providers)
    {
        Providers = providers;
    }

    public TUserProvidersDto Providers { get; set; }
}