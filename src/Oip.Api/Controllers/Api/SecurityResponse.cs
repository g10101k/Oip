namespace Oip.Api.Controllers.Api;

/// <summary>
/// Security dto
/// </summary>
public class SecurityResponse
{
    /// <summary>
    /// Code
    /// </summary>
    public string Code { get; set; } = default!;

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    /// Roles
    /// </summary>
    public List<string>? Roles { get; set; }
}