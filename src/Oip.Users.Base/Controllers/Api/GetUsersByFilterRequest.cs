namespace Oip.Users.Base.Controllers.Api;

/// <summary>
/// Page and search text of the users list.
/// </summary>
public class GetUsersByFilterRequest
{
    /// <summary>
    /// Number of users to skip.
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Number of users to return, from 1 to 1000.
    /// </summary>
    public int Take { get; set; } = 20;

    /// <summary>
    /// Text searched in the e-mail, first name and last name, ignoring case.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Activity of the users to return; users of both kinds are returned when it is <c>null</c>.
    /// </summary>
    public bool? IsActive { get; set; }
}
