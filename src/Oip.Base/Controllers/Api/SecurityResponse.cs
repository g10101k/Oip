namespace Oip.Api.Controllers.Api;

/// <summary>
/// Security dto
/// </summary>
public class SecurityResponse
{
    /// <summary>
    /// Code
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Roles
    /// </summary>
    public List<string>? Roles { get; set; }
}