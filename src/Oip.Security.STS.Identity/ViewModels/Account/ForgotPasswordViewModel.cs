using System.ComponentModel.DataAnnotations;
using Oip.Security.Shared.Configuration.Configuration.Identity;

namespace Oip.Security.STS.Identity.ViewModels.Account;

public class ForgotPasswordViewModel
{
    [Required] public LoginResolutionPolicy? Policy { get; set; }

    [EmailAddress] public string Email { get; set; }

    public string Username { get; set; }
}