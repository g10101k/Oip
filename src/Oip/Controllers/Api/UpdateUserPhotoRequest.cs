namespace Oip.Controllers.Api;

/// <summary>
/// Update user photo request
/// </summary>
public class UpdateUserPhotoRequest
{
    /// <summary>
    /// Photo
    /// </summary>
    public byte[] Photo { get; set; } = null!;
}