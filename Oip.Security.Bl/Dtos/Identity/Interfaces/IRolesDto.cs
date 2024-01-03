using System.Collections.Generic;

namespace Oip.Security.Bl.Dtos.Identity.Interfaces;

public interface IRolesDto
{
    int PageSize { get; set; }
    int TotalCount { get; set; }
    List<IRoleDto> Roles { get; }
}