using Oip.Security.Bl.Dtos.Identity.Interfaces;

namespace Oip.Security.Bl.Dtos.Identity.Base;

public class BaseUserChangePasswordDto<TUserId> : IBaseUserChangePasswordDto
{
    public TUserId UserId { get; set; }

    object IBaseUserChangePasswordDto.UserId => UserId;
}