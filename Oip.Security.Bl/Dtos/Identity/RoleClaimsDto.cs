using System.Collections.Generic;
using System.Linq;
using Oip.Security.Bl.Dtos.Identity.Interfaces;

namespace Oip.Security.Bl.Dtos.Identity;

public class RoleClaimsDto<TRoleClaimDto, TKey> : RoleClaimDto<TKey>, IRoleClaimsDto
    where TRoleClaimDto : RoleClaimDto<TKey>
{
    public RoleClaimsDto()
    {
        Claims = new List<TRoleClaimDto>();
    }

    public List<TRoleClaimDto> Claims { get; set; }

    public string RoleName { get; set; }

    public int TotalCount { get; set; }

    public int PageSize { get; set; }

    List<IRoleClaimDto> IRoleClaimsDto.Claims => Claims.Cast<IRoleClaimDto>().ToList();
}