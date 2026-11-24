using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Oip.Base.Security.Objects.Dtos;
using Oip.Base.Services;

namespace Oip.Base.Security.Objects;

/// <summary>
/// Finds subjects to give rights to and describes the subjects of an access control list.
/// </summary>
public interface ISubjectDirectory
{
    /// <summary>
    /// Finds subjects whose name contains the filter.
    /// </summary>
    Task<IReadOnlyList<SubjectDto>> SearchAsync(string? filter, int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Describes the subjects behind the keys. A key missing from the result is unknown.
    /// </summary>
    Task<IReadOnlyDictionary<string, SubjectDto>> DescribeAsync(IReadOnlyCollection<string> keys,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Subject directory backed by the realm roles and users of Keycloak.
/// </summary>
public sealed class KeycloakSubjectDirectory(
    KeycloakService keycloakService,
    IMemoryCache cache,
    ILogger<KeycloakSubjectDirectory> logger) : ISubjectDirectory
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private const string RolesCacheKey = "Oip.AccessControl.Roles";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SubjectDto>> SearchAsync(string? filter, int limit,
        CancellationToken cancellationToken = default)
    {
        var result = new List<SubjectDto>();
        result.AddRange(new[] { Pseudo(SubjectKeys.Everyone), Pseudo(SubjectKeys.Owner) }
            .Where(s => Matches(s.DisplayName, filter)));

        var roles = await GetRolesAsync();
        result.AddRange(roles.Where(r => Matches(r, filter)).Select(Role));

        try
        {
            var users = await keycloakService.GetUsersAsync(0, limit,
                string.IsNullOrWhiteSpace(filter) ? null : filter.Trim());
            result.AddRange(users.Where(u => u.Id is not null).Select(u => User(u.Id!, Describe(u))));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Keycloak users are not available for the access control subject search.");
        }

        return result.Take(limit).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, SubjectDto>> DescribeAsync(IReadOnlyCollection<string> keys,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, SubjectDto>(StringComparer.Ordinal);
        var roles = await GetRolesAsync();
        var rolesAvailable = roles.Count > 0;

        foreach (var key in keys.Distinct(StringComparer.Ordinal))
        {
            var value = SubjectKeys.GetValue(key);
            switch (SubjectKeys.GetType(key))
            {
                case SubjectKeys.Everyone or SubjectKeys.Owner:
                    result[key] = Pseudo(key);
                    break;
                case SubjectKeys.RoleType when !rolesAvailable || roles.Contains(value):
                    result[key] = Role(value);
                    break;
                case SubjectKeys.UserType:
                    var user = await DescribeUserAsync(value);
                    if (user is not null)
                        result[key] = user;
                    break;
                case SubjectKeys.ClientType or SubjectKeys.GroupType:
                    result[key] = new SubjectDto
                        { SubjectKey = key, SubjectType = SubjectKeys.GetType(key), DisplayName = value };
                    break;
            }
        }

        return result;
    }

    private async Task<SubjectDto?> DescribeUserAsync(string subject)
    {
        var cacheKey = $"Oip.AccessControl.User.{subject}";
        if (cache.TryGetValue(cacheKey, out SubjectDto? cached))
            return cached;

        try
        {
            var user = await keycloakService.GetUserAsync(subject);
            var dto = user is null ? null : User(subject, Describe(user));
            cache.Set(cacheKey, dto, CacheDuration);
            return dto;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Keycloak user {Subject} could not be described.", subject);
            return User(subject, subject);
        }
    }

    private async Task<HashSet<string>> GetRolesAsync()
    {
        if (cache.TryGetValue(RolesCacheKey, out HashSet<string>? cached) && cached is not null)
            return cached;

        try
        {
            var roles = (await keycloakService.GetRealmRoles()).Select(r => r.Name).ToHashSet(StringComparer.Ordinal);
            cache.Set(RolesCacheKey, roles, CacheDuration);
            return roles;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Keycloak roles are not available for the access control.");
            return [];
        }
    }

    private static string Describe(Clients.UserRepresentation user)
    {
        var name = string.Join(' ', new[] { user.FirstName, user.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
        var login = user.Username ?? user.Email ?? user.Id ?? string.Empty;
        return string.IsNullOrWhiteSpace(name) ? login : $"{name} ({login})";
    }

    private static bool Matches(string value, string? filter) =>
        string.IsNullOrWhiteSpace(filter) || value.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase);

    private static SubjectDto Pseudo(string key) =>
        new() { SubjectKey = key, SubjectType = key, DisplayName = key };

    private static SubjectDto Role(string name) =>
        new() { SubjectKey = SubjectKeys.Role(name), SubjectType = SubjectKeys.RoleType, DisplayName = name };

    private static SubjectDto User(string subject, string displayName) =>
        new() { SubjectKey = SubjectKeys.User(subject), SubjectType = SubjectKeys.UserType, DisplayName = displayName };
}
