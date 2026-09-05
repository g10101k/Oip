using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    /// Options used to read user settings. Property names are matched case-insensitively so that settings
    /// persisted before the switch to <see cref="JsonSerializer"/> keep deserializing.
    /// </summary>
    private static readonly JsonSerializerOptions SettingsJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Gets current user photo.
    /// </summary>
    /// <returns>User photo image or not found response.</returns>
    [Authorize, HttpGet("get-user-photo")]
    [Produces("image/jpeg", "image/png", "image/gif", "image/webp", "application/json")]
    [ProducesResponseType<FileStreamResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserPhoto(CancellationToken cancellationToken)
    {
        var subject = claimService.GetUserSubject();
        if (string.IsNullOrWhiteSpace(subject))
        {
            return Unauthorized(new ApiExceptionResponse("Unauthorized", "Current user subject is not available.",
                StatusCodes.Status401Unauthorized));
        }

        var user = await userRepository.GetBySubjectAsync(subject, cancellationToken);
        return await GetUserPhotoResultAsync(user, cancellationToken);
    }

    /// <summary>
    /// Gets user photo by user identifier.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User photo image or not found response.</returns>
    [Authorize, HttpGet("get-user-photo-by-id/{userId:int}")]
    [Produces("image/jpeg", "image/png", "image/gif", "image/webp", "application/json")]
    [ProducesResponseType<FileStreamResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserPhotoById(int userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
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

        var subject = claimService.GetUserSubject();
        if (string.IsNullOrWhiteSpace(subject))
        {
            return Unauthorized(new ApiExceptionResponse("Unauthorized", "Current user subject is not available.",
                StatusCodes.Status401Unauthorized));
        }

        var user = await userRepository.GetOrCreateBySubjectAsync(subject, claimService.GetUserEmail(), cancellationToken);
        var storedPhoto = await userPhotoStorage.SaveAsync(user.UserId, files, cancellationToken);
        await userRepository.UpdateUserPhotoMetadataAsync(
            user.UserId,
            storedPhoto.ObjectName,
            storedPhoto.ContentType,
            cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Deletes the current user's photo.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>OK result</returns>
    [Authorize, HttpDelete("delete-user-photo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUserPhoto(CancellationToken cancellationToken)
    {
        var subject = claimService.GetUserSubject();
        if (string.IsNullOrWhiteSpace(subject))
        {
            return Unauthorized(new ApiExceptionResponse("Unauthorized", "Current user subject is not available.",
                StatusCodes.Status401Unauthorized));
        }

        var user = await userRepository.GetBySubjectAsync(subject, cancellationToken);
        if (user == null || string.IsNullOrWhiteSpace(user.PhotoObjectName))
        {
            return Ok();
        }

        await userPhotoStorage.DeleteAsync(user.PhotoObjectName, cancellationToken);
        await userRepository.ClearUserPhotoMetadataAsync(user.UserId, cancellationToken);

        return Ok();
    }

    /// <summary>
    /// Get settings of the current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    [Authorize, HttpGet("get-settings")]
    [ProducesResponseType<UserSettingsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserSettingsDto>> GetSettings(CancellationToken cancellationToken)
    {
        var subject = claimService.GetUserSubject();
        if (string.IsNullOrWhiteSpace(subject))
        {
            return Unauthorized(new ApiExceptionResponse("Unauthorized", "Current user subject is not available.",
                StatusCodes.Status401Unauthorized));
        }

        var json = await userRepository.GetUserSettingsAsync(subject, cancellationToken);
        if (string.IsNullOrWhiteSpace(json))
            return new UserSettingsDto();

        return JsonSerializer.Deserialize<UserSettingsDto>(json, SettingsJsonOptions) ?? new UserSettingsDto();
    }

    /// <summary>
    /// Update User settings
    /// </summary>
    /// <param name="settings">Settings</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Authorize, HttpPut("set-settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSettings(UserSettingsDto settings, CancellationToken cancellationToken)
    {
        var subject = claimService.GetUserSubject();
        if (string.IsNullOrWhiteSpace(subject))
        {
            return Unauthorized(new ApiExceptionResponse("Unauthorized", "Current user subject is not available.",
                StatusCodes.Status401Unauthorized));
        }

        var json = JsonSerializer.Serialize(settings);
        await userRepository.UpdateUserSettingsAsync(subject, json, cancellationToken);
        return Ok();
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
