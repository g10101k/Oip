using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Oip.Base.Data.Constants;
using Oip.Base.Security.Objects.Data;

namespace Oip.Base.Security.Objects;

/// <summary>
/// Builds and parses subject keys. A key is {type}:{value}, except the pseudo subjects everyone and owner.
/// </summary>
public static partial class SubjectKeys
{
    /// <summary>
    /// Every authenticated user.
    /// </summary>
    public const string Everyone = "everyone";

    /// <summary>
    /// The owner of the object, see <see cref="SecurableObjectBuilder{TEntity}.Owner" />.
    /// </summary>
    public const string Owner = "owner";

    /// <summary>
    /// Realm role type.
    /// </summary>
    public const string RoleType = "role";

    /// <summary>
    /// User type, the value is the Keycloak subject.
    /// </summary>
    public const string UserType = "user";

    /// <summary>
    /// Service account type, the value is the client id.
    /// </summary>
    public const string ClientType = "client";

    /// <summary>
    /// Group type, the value is the Keycloak group id.
    /// </summary>
    public const string GroupType = "group";

    /// <summary>
    /// Key of a realm role.
    /// </summary>
    public static string Role(string name) => $"{RoleType}:{name}";

    /// <summary>
    /// Key of a user.
    /// </summary>
    public static string User(string subject) => $"{UserType}:{subject}";

    /// <summary>
    /// Key of a service account.
    /// </summary>
    public static string Client(string clientId) => $"{ClientType}:{clientId}";

    /// <summary>
    /// Returns the type of a key: role, user, client, group, everyone or owner.
    /// </summary>
    public static string GetType(string key)
    {
        var separator = key.IndexOf(':');
        return separator < 0 ? key : key[..separator];
    }

    /// <summary>
    /// Returns the value of a key, or the key itself for a pseudo subject.
    /// </summary>
    public static string GetValue(string key)
    {
        var separator = key.IndexOf(':');
        return separator < 0 ? key : key[(separator + 1)..];
    }

    /// <summary>
    /// Checks that the key has a known type and a non-empty value.
    /// </summary>
    public static bool IsValid(string? key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > AccessControlModelBuilderExtensions.SubjectKeyMaxLength)
            return false;
        return key is Everyone or Owner || KeyRegex().IsMatch(key);
    }

    [GeneratedRegex("^(role|user|client|group):\\S.*$")]
    private static partial Regex KeyRegex();
}

/// <summary>
/// Subjects of the current request.
/// </summary>
/// <param name="Keys">Keys of the subjects the caller acts as. The owner pseudo subject is never listed here.</param>
/// <param name="UserSubject">Keycloak subject of the caller, matched against the owner of an object.</param>
/// <param name="IsAuthenticated">Whether the caller is authenticated.</param>
/// <param name="IsAdmin">Whether the caller holds the admin role and bypasses the access control.</param>
public sealed record SubjectSet(
    IReadOnlyList<string> Keys,
    string? UserSubject,
    bool IsAuthenticated,
    bool IsAdmin)
{
    /// <summary>
    /// Anonymous caller without any subject.
    /// </summary>
    public static SubjectSet Anonymous { get; } = new([], null, false, false);
}

/// <summary>
/// Resolves the subjects of the current request.
/// </summary>
public interface ISubjectResolver
{
    /// <summary>
    /// Returns the subjects of the current request.
    /// </summary>
    SubjectSet Resolve();
}

/// <summary>
/// Adds application specific subject keys, for example departments kept in the application database.
/// </summary>
public interface ISubjectContributor
{
    /// <summary>
    /// Returns additional subject keys of the user.
    /// </summary>
    IEnumerable<string> GetSubjectKeys(ClaimsPrincipal user);
}

/// <summary>
/// Default resolver: roles, the user subject, the service account client and everyone, taken from the claims.
/// </summary>
public sealed class ClaimsSubjectResolver(
    IHttpContextAccessor httpContextAccessor,
    IEnumerable<ISubjectContributor> contributors) : ISubjectResolver
{
    private const string ClientIdClaim = "client_id";
    private const string SubjectClaim = "sub";
    private const string ItemKey = "Oip.AccessControl.SubjectSet";

    /// <inheritdoc />
    public SubjectSet Resolve()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
            return SubjectSet.Anonymous;

        if (httpContext.Items.TryGetValue(ItemKey, out var cached) && cached is SubjectSet cachedSet)
            return cachedSet;

        var set = Resolve(httpContext.User, contributors);
        httpContext.Items[ItemKey] = set;
        return set;
    }

    /// <summary>
    /// Resolves the subjects of a principal.
    /// </summary>
    public static SubjectSet Resolve(ClaimsPrincipal user, IEnumerable<ISubjectContributor>? contributors = null)
    {
        if (user.Identity?.IsAuthenticated != true)
            return SubjectSet.Anonymous;

        var keys = new List<string> { SubjectKeys.Everyone };
        var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).Distinct().ToList();
        keys.AddRange(roles.Select(SubjectKeys.Role));

        var subject = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue(SubjectClaim);
        if (!string.IsNullOrWhiteSpace(subject))
            keys.Add(SubjectKeys.User(subject));

        var clientId = user.FindFirstValue(ClientIdClaim);
        if (!string.IsNullOrWhiteSpace(clientId))
            keys.Add(SubjectKeys.Client(clientId));

        if (contributors is not null)
            keys.AddRange(contributors.SelectMany(c => c.GetSubjectKeys(user)).Where(SubjectKeys.IsValid));

        return new SubjectSet(keys.Distinct(StringComparer.Ordinal).ToList(), subject, true,
            roles.Contains(SecurityConstants.AdminRole));
    }
}
