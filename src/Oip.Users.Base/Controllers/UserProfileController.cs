using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Oip.Base.Exceptions;
using Oip.Base.Services;
using Oip.Users.Base.Data.Repositories;
using Oip.Users.Base.Services;

namespace Oip.Users.Base.Controllers;

/// <summary>
/// Controller for managing user profile operations
/// </summary>
[ApiController]
[Route("api/user-profile")]
[ApiExplorerSettings(GroupName = "users")]
public class UserProfileController(
    ClaimService claimService,
    UserRepository userRepository,
    IUserPhotoStorage userPhotoStorage) : ControllerBase
{
    /// <summary>
    /// Gets current user photo.
    /// </summary>
    /// <returns>User photo image or not found response.</returns>
    [Authorize, HttpGet("get-user-photo")]
    [Produces("image/jpeg", "image/png", "image/gif", "image/webp")]
    [ProducesResponseType<FileStreamResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserPhoto(CancellationToken cancellationToken)
    {
        var email = claimService.GetUserEmail();
        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new ApiExceptionResponse("Unauthorized", "Current user email is not available.",
                StatusCodes.Status401Unauthorized));
        }

        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        return await GetUserPhotoResultAsync(user, cancellationToken);
    }

    /// <summary>
    /// Gets user photo by user identifier.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User photo image or not found response.</returns>
    [Authorize, HttpGet("get-user-photo-by-id/{userId:int}")]
    [Produces("image/jpeg", "image/png", "image/gif", "image/webp")]
    [ProducesResponseType<FileStreamResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserPhotoById(int userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId);
        return await GetUserPhotoResultAsync(user, cancellationToken);
    }

    /// <summary>
    /// Uploads user photo
    /// </summary>
    /// <param name="files">Photo file to upload</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>OK result</returns>
    [Authorize, HttpPost("post-user-photo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> OnPostUploadAsync(IFormFile? files, CancellationToken cancellationToken)
    {
        if (files == null || files.Length == 0)
        {
            return BadRequest(new ApiExceptionResponse("Invalid photo", "Photo file is required.",
                StatusCodes.Status400BadRequest));
        }

        if (!files.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new ApiExceptionResponse("Invalid photo", "Only image files are allowed.",
                StatusCodes.Status400BadRequest));
        }

        var email = claimService.GetUserEmail();
        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new ApiExceptionResponse("Unauthorized", "Current user email is not available.",
                StatusCodes.Status401Unauthorized));
        }

        var user = await userRepository.GetOrCreateByEmailAsync(email, cancellationToken);
        var storedPhoto = await userPhotoStorage.SaveAsync(user.UserId, files, cancellationToken);
        await userRepository.UpdateUserPhotoMetadataAsync(
            user.UserId,
            storedPhoto.ObjectName,
            storedPhoto.ContentType,
            cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Get user setting by e-mail
    /// </summary>
    /// <returns></returns>
    [Authorize, HttpGet("get-settings")]
    [ProducesResponseType<UserSettingsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<UserSettingsDto> GetSettings()
    {
        var json = userRepository.GetUserSettings(claimService.GetUserEmail()!);

        return JsonConvert.DeserializeObject<UserSettingsDto>(json) ?? new();
    }

    /// <summary>
    /// Update User settings
    /// </summary>
    /// <param name="settings">Settings</param>
    [Authorize, HttpPut("set-settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task UpdateSettings(UserSettingsDto settings)
    {
        var json = JsonConvert.SerializeObject(settings);
        await userRepository.UpdateUserSettings(claimService.GetUserEmail()!, json);
    }

    private async Task<IActionResult> GetUserPhotoResultAsync(
        Data.Entities.UserEntity? user,
        CancellationToken cancellationToken)
    {
        if (user == null)
        {
            return NotFound(new ApiExceptionResponse("Photo not found", "User photo was not found.",
                StatusCodes.Status404NotFound));
        }

        if (!string.IsNullOrWhiteSpace(user.PhotoObjectName))
        {
            var content = await userPhotoStorage.OpenReadAsync(
                user.PhotoObjectName,
                user.PhotoContentType ?? "image/jpeg",
                cancellationToken);
            return File(content.Content, content.ContentType);
        }

        return NotFound(new ApiExceptionResponse("Photo not found", "User photo was not found.",
            StatusCodes.Status404NotFound));
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
