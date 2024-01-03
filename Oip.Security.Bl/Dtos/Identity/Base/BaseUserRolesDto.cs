using Oip.Security.Bl.Dtos.Identity.Interfaces;

namespace Oip.Security.Bl.Dtos.Identity.Base;

public class BaseUserRolesDto<TKey> : IBaseUserRolesDto
{
    public TKey UserId { get; set; }

    public TKey RoleId { get; set; }

    object IBaseUserRolesDto.UserId => UserId;

    object IBaseUserRolesDto.RoleId => RoleId;
}