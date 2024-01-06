using System.ComponentModel.DataAnnotations;
using Oip.Security.Bl.Dtos.Identity.Base;
using Oip.Security.Bl.Dtos.Identity.Interfaces;

namespace Oip.Security.Bl.Dtos.Identity;

public class UserChangePasswordDto<TKey> : BaseUserChangePasswordDto<TKey>, IUserChangePasswordDto
{
    public string UserName { get; set; }

    [Required] public string Password { get; set; }

    [Required] [Compare(nameof(Password))] public string ConfirmPassword { get; set; }
}