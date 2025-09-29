using Oip.Base.Clients;
using UserRepresentation = Oip.Users.Clients.UserRepresentation;

namespace Oip.Users.Entities;

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
    /// User photo
    /// </summary>
    public byte[]? Photo { get; set; }

    /// <summary>
    /// Updates the user entity from Keycloak user representation
    /// </summary>
    /// <param name="userRep">Keycloak user representation</param>
    public void UpdateFromKeycloak(UserRepresentation userRep)
    {
        Email = userRep.Email ?? Email;
        FirstName = userRep.FirstName ?? FirstName;
        LastName = userRep.LastName ?? LastName;
        IsActive = userRep.Enabled ?? IsActive;
        UpdatedAt = DateTimeOffset.UtcNow;
        LastSyncedAt = DateTimeOffset.UtcNow;
    }
}