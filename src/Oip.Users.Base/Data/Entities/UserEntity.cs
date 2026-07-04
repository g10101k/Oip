namespace Oip.Users.Base.Data.Entities;

/// <summary>
/// User entity
/// </summary>
public class UserEntity
{
    /// <summary>
    /// User id
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the Keycloak identifier for the user.
    /// </summary>
    public string KeycloakId { get; set; } = string.Empty;

    /// <summary>
    /// E-mail
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// First name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the user is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Creation date and time
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Last update date and time
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Last synchronization date and time
    /// </summary>
    public DateTimeOffset LastSyncedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// User photo object name in object storage.
    /// </summary>
    public string? PhotoObjectName { get; set; }

    /// <summary>
    /// User photo content type.
    /// </summary>
    public string? PhotoContentType { get; set; }

    /// <summary>
    /// User settings in json
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Settings { get; set; } = null!;

    /// <summary>
    /// Physical extension row for custom fields.
    /// </summary>
    public UserExtensionEntity? Extension { get; set; }
}
