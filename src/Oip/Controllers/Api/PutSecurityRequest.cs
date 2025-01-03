namespace Oip.Controllers.Api;

/// <summary>
/// Put security dto
/// </summary>
public class PutSecurityRequest
{
    /// <summary>
    /// Instance id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Securities
    /// </summary>
    public List<SecurityResponse> Securities { get; set; } = new();
}