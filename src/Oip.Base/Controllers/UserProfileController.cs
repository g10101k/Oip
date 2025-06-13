using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Data.Repositories;
using Oip.Base.Services;

namespace Oip.Base.Controllers;

/// <summary>
/// Security controller
/// </summary>
[ApiController]
[Route("api/user-profile")]
public class UserProfileController : ControllerBase
{
    private readonly UserService _userService;
    private readonly UserRepository _userRepository;

    /// <summary>.ctor</summary>
    public UserProfileController(UserService userService, UserRepository userRepository)
    {
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
        var userDto = _userRepository.GetUserByEmail(email);
        if (userDto?.Photo != null)
            return new FileContentResult(userDto.Photo, "image/jpeg");
        return new NoContentResult();
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
            await ms.WriteAsync(buffer, 0, read);
        }

        _userRepository.UpsertUserPhoto(_userService.GetUserEmail()!, ms.ToArray());

        return Ok();
    }
}