using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Services;
using Oip.Data.Repositories;

namespace Oip.Controllers;

/// <summary>
/// Security controller
/// </summary>
[ApiController]
[Route("api/user-profile")]
public class UserProfileController : ControllerBase
{
    private readonly KeycloakService _keycloakService;
    private readonly UserService _userService;
    private readonly UserRepository _userRepository;

    /// <summary>.ctor</summary>
    public UserProfileController(KeycloakService keycloakService, UserService userService,
        UserRepository userRepository)
    {
        _keycloakService = keycloakService;
        _userService = userService;
        _userRepository = userRepository;
    }

    /// <summary> 
    /// Get all roles
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-user-photo")]
    [Authorize]
    public async Task<IActionResult> GetUserPhoto(string email)
    {
        var q = _userRepository.GetUserByEmail(email);
        if (q?.Photo != null)
            return new FileContentResult(q.Photo, "image/jpeg");
        return new NotFoundResult();
    }

    /// <summary> 
    /// Get all roles
    /// </summary>
    /// <returns></returns>
    [HttpPost("post-user-photo")]
    [Authorize]
    public async Task<IActionResult> OnPostUploadAsync(IFormFile files)
    {
        await using var stream = files.OpenReadStream();
        var buffer = new byte[16 * 1024];
        using var ms = new MemoryStream();
        int read;
        while ((read = await stream.ReadAsync(buffer)) > 0)
        {
            ms.Write(buffer, 0, read);
        }

        _userRepository.UpsertUserPhoto(_userService.GetUserEmail()!, ms.ToArray());

        return Ok();
    }
}