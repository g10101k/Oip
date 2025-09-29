namespace Oip.Base.Data.Entities;

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
    /// E-mail
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// User photo
    /// </summary>
    public byte[]? Photo { get; set; }
}