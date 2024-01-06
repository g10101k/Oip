using Oip.Security.Bl.Dtos.Identity.Interfaces;

namespace Oip.Security.Bl.Dtos.Identity.Base;

public class BaseUserProviderDto<TUserId> : IBaseUserProviderDto
{
    public TUserId UserId { get; set; }

    object IBaseUserProviderDto.UserId => UserId;
}