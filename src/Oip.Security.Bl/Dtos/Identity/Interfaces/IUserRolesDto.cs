using System.Collections.Generic;
using Oip.Security.Bl.Dtos.Common;

namespace Oip.Security.Bl.Dtos.Identity.Interfaces;

public interface IUserRolesDto : IBaseUserRolesDto
{
    string UserName { get; set; }
    List<SelectItemDto> RolesList { get; set; }
    List<IRoleDto> Roles { get; }
    int PageSize { get; set; }
    int TotalCount { get; set; }
}