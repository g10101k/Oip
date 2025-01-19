namespace Oip.Data.Entities;

/// <summary>
/// User entity
/// </summary>
public class UserEntity
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public byte[]?  Photo { get; set; }
}