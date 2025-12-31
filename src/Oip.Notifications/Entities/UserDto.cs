namespace Oip.Notifications.Entities;

/// <summary>
/// Represents a system user
/// </summary>
public class UserDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the user
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the username of the user
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// Gets or sets the email address of the user
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets the phone number of the user (optional)
    /// </summary>
    public string? PhoneNumber { get; set; }
}