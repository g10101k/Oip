using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Api;
using Oip.Base.Clients;
using Oip.Base.Helpers;
using Oip.Base.Services;
using Oip.Controllers.Api;
using Oip.Data.Repositories;
using Oip.Settings;

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
    public async Task<FileResult> GetUserPhoto()
    {
        var q = _userRepository.GetUserByEmail(_userService.GetUserEmail()!);
        if (q?.Photo != null)
            return new FileContentResult(q.Photo, "image/jpeg");
        return new FileContentResult([], "image/jpeg");
    }

    /// <summary> 
    /// Get all roles
    /// </summary>
    /// <returns></returns>
    [HttpPost("put-user-photo")]
    [Authorize]
    public async Task<IActionResult> OnPostUploadAsync(IFormFile files)
    {
        using var stream = files.OpenReadStream();
        byte[] buffer = new byte[16 * 1024];
        using MemoryStream ms = new MemoryStream();
        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            ms.Write(buffer, 0, read);
        }

        _userRepository.UpsertUserPhoto(_userService.GetUserEmail()!, ms.ToArray());

        return Ok();
    }

    /// <summary> 
    /// Get all roles
    /// </summary>
    /// <returns></returns>
    [HttpPost("put-user-photo2")]
    [Authorize]
    public async Task<IActionResult> OnPostUploadAsync2(List<IFormFile> files)
    {
        long size = files.Sum(f => f.Length);

        foreach (var formFile in files)
        {
            if (formFile.Length > 0)
            {
                var filePath = Path.GetTempFileName();

                using (var stream = System.IO.File.Create(filePath))
                {
                    await formFile.CopyToAsync(stream);
                }
            }
        }

        // Process uploaded files
        // Don't rely on or trust the FileName property without validation.

        return Ok(new { count = files.Count, size });
    }
}