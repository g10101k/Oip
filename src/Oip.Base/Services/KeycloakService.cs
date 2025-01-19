using Oip.Base.Clients;
using Oip.Base.Settings;

namespace Oip.Base.Services;

/// <summary>
/// Keycloak Service
/// </summary>
public class KeycloakService
{
    private readonly KeycloakClient _keycloakClient;
    private readonly SecurityServiceSettings _appSettings;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="keycloakClient"></param>
    /// <param name="appSettings"></param>
    public KeycloakService(KeycloakClient keycloakClient, IBaseOipModuleAppSettings appSettings)
    {
        _keycloakClient = keycloakClient;
        _appSettings = appSettings.SecurityService;
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    /// <returns></returns>
    public async Task<List<Role>> GetRealmRoles()
    {
        _ = await _keycloakClient.Authentication(_appSettings.ClientId,
            _appSettings.ClientSecret, _appSettings.Realm, CancellationToken.None);

        return await _keycloakClient.GetRoles(_appSettings.Realm, CancellationToken.None);
    }
}