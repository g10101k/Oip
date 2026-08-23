namespace Oip.Base.Data.Constants;

/// <summary>
/// Defines a set of standard security-related constants 
/// used throughout the application, including permissions and roles.
/// </summary>
public static class SecurityConstants
{
    /// <summary>
    /// Permission to read or view content.
    /// </summary>
    public const string Read = "read";

    /// <summary>
    /// Permission to modify or update content.
    /// </summary>
    public const string Edit = "edit";

    /// <summary>
    /// Permission to remove or delete content.
    /// </summary>
    public const string Delete = "delete";

    /// <summary>
    /// Role that grants full administrative access, including user and settings management.
    /// </summary>
    public const string AdminRole = "admin";

    /// <summary>
    /// Header carrying the module instance identifier for endpoints that do not accept it explicitly.
    /// </summary>
    public const string ModuleInstanceIdHeader = "X-Module-Instance-Id";
}