using System.Security.Claims;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Contexts;
using Oip.Base.Data.Entities;

namespace Oip.Base.Security.Objects;

/// <summary>
/// Access control of the module instances: the menu items, folders included. An instance inherits the rights of its
/// folder, and the rights besides the standard ones are the ones the modules declare in <c>GetModuleRights</c>.
/// </summary>
public static class ModuleAccessControl
{
    /// <summary>
    /// Object type of a module instance.
    /// </summary>
    public const string ModuleInstanceType = "oip.module-instance";

    /// <summary>
    /// Registers the module instance type.
    /// </summary>
    public static void Configure(AccessControlOptions<OipModuleContext> options)
    {
        options.Object<ModuleInstanceEntity>(ModuleInstanceType, o => o
            .Id(e => e.ModuleInstanceId)
            .Parent(ModuleInstanceType, e => e.ParentId)
            .StoredRights());
    }

    /// <summary>
    /// Reference to a module instance.
    /// </summary>
    public static ObjectRef Instance(long moduleInstanceId) => new(ModuleInstanceType, moduleInstanceId);

    /// <summary>
    /// Subjects of a module instance check: everyone, the roles and the user. Groups and service account clients
    /// are not taken into account for the module instances.
    /// </summary>
    public static SubjectSet SubjectsOf(IReadOnlyCollection<string> roles, string? userSubject = null)
    {
        if (roles.Count == 0 && userSubject is null)
            return SubjectSet.Anonymous;

        var keys = new List<string> { SubjectKeys.Everyone };
        keys.AddRange(roles.Distinct().Select(SubjectKeys.Role));
        if (!string.IsNullOrWhiteSpace(userSubject))
            keys.Add(SubjectKeys.User(userSubject));
        return new SubjectSet(keys, userSubject, true, roles.Contains(SecurityConstants.AdminRole));
    }

    /// <summary>
    /// Subjects of a module instance check on behalf of the principal: everyone, its roles and its user.
    /// </summary>
    public static SubjectSet SubjectsOf(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
            return SubjectSet.Anonymous;

        var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        var subject = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return SubjectsOf(roles, subject);
    }
}
