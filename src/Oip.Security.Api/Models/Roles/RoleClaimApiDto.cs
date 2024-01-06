using System.ComponentModel.DataAnnotations;

namespace Oip.Security.Api.Models.Roles;

public class RoleClaimApiDto<TKey>
{
    public int ClaimId { get; set; }

    public TKey RoleId { get; set; }

    [Required] public string ClaimType { get; set; }


    [Required] public string ClaimValue { get; set; }
}