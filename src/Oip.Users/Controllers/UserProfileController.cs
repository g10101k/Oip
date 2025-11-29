using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Oip.Base.Exceptions;
using Oip.Base.Services;
using Oip.Users.Repositories;

namespace Oip.Users.Controllers;

/// <summary>
/// Controller for managing user profile operations
/// </summary>
[ApiController]
[Route("api/user-profile")]
[ApiExplorerSettings(GroupName = "users")]
public class UserProfileController(UserService userService, UserRepository userRepository) : ControllerBase
{
    /// <summary>
    /// Gets user photo by email address
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>User photo as JPEG image or NotFound result</returns>
    [HttpGet("get-user-photo")]
    [Authorize]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<OipException>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<OipException>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<OipException>(StatusCodes.Status500InternalServerError)]
    public IActionResult GetUserPhoto(string email)
    {
        var userDto = userRepository.GetUserByEmail(email);
        if (userDto?.Photo != null)
            return new FileContentResult(userDto.Photo, "image/jpeg");
        return new NotFoundResult();
    }

    /// <summary>
    /// Uploads user photo
    /// </summary>
    /// <param name="files">Photo file to upload</param>
    /// <returns>OK result</returns>
    [HttpPost("post-user-photo")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<OipException>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<OipException>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<OipException>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> OnPostUploadAsync(IFormFile files)
    {
        await using var stream = files.OpenReadStream();
        var buffer = new byte[16 * 1024];
        using var ms = new MemoryStream();
        int read;
        while ((read = await stream.ReadAsync(buffer)) > 0)
        {
            await ms.WriteAsync(buffer.AsMemory(0, read), CancellationToken.None);
        }

        userRepository.UpsertUserPhoto(userService.GetUserEmail()!, ms.ToArray());

        return Ok();
    }

    /// <summary>
    /// Get user setting by e-mail
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-settings")]
    [Authorize]
    [ProducesResponseType<UserSettingsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<OipException>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<OipException>(StatusCodes.Status500InternalServerError)]
    public async Task<UserSettingsDto> GetSettings()
    {
        var json = userRepository.GetUserSettings(userService.GetUserEmail()!);

        return JsonConvert.DeserializeObject<UserSettingsDto>(json) ?? new();
    }

    /// <summary>
    /// Update User settings
    /// </summary>
    /// <param name="settings">Settings</param>
    [HttpPut("set-settings")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<OipException>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<OipException>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<OipException>(StatusCodes.Status500InternalServerError)]
    public async Task UpdateSettings(UserSettingsDto settings)
    {
        var json = JsonConvert.SerializeObject(settings);
        await userRepository.UpdateUserSettings(userService.GetUserEmail()!, json);
    }
}

/// <summary>
/// Represents user interface and localization settings.
/// </summary>
public class UserSettingsDto
{
    /// <summary>
    /// Gets or sets the selected visual preset name.
    /// </summary>
    public string Preset { get; set; } = "Aura";

    /// <summary>
    /// Gets or sets the primary color theme.
    /// </summary>
    public string Primary { get; set; } = "emerald";

    /// <summary>
    /// Gets or sets the surface color. Can be null.
    /// </summary>
    public string? Surface { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether the dark theme is enabled.
    /// </summary>
    public bool DarkTheme { get; set; } = false;

    /// <summary>
    /// Gets or sets the layout mode for the menu (e.g., static, overlay).
    /// </summary>
    public string MenuMode { get; set; } = "static";

    /// <summary>
    /// Gets or sets the selected language code.
    /// </summary>
    public string Language { get; set; } = "en";

    /// <summary>
    /// Gets or sets the date format pattern.
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// Gets or sets the time format pattern.
    /// </summary>
    public string TimeFormat { get; set; } = "HH:mm:ss";

    /// <summary>
    /// Gets or sets the user's time zone.
    /// </summary>
    public string TimeZone { get; set; } = "Europe/Moscow";
}