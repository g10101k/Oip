using System.ComponentModel.DataAnnotations;
using Oip.Security.Bl.Dtos.Identity.Base;
using Oip.Security.Bl.Dtos.Identity.Interfaces;

namespace Oip.Security.Bl.Dtos.Identity;

public class RoleClaimDto<TKey> : BaseRoleClaimDto<TKey>, IRoleClaimDto
{
    [Required] public string ClaimType { get; set; }


    [Required] public string ClaimValue { get; set; }
}