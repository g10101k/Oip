namespace Oip.Users.Base.Data.Entities;

/// <summary>
/// Physical extension row for user-specific custom columns.
/// </summary>
public class UserExtensionEntity
{
    /// <summary>
    /// User primary key and extension row primary key.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Base user row.
    /// </summary>
    public UserEntity User { get; set; } = null!;
}
