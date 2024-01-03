using System.ComponentModel.DataAnnotations;
using Oip.Security.Bl.Dtos.Identity.Base;
using Oip.Security.Bl.Dtos.Identity.Interfaces;

namespace Oip.Security.Bl.Dtos.Identity;

public class RoleDto<TKey> : BaseRoleDto<TKey>, IRoleDto
{
    [Required] public string Name { get; set; }
}