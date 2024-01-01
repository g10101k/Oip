namespace Oip.Base.Api;

/// <summary>
/// Routing for ModuleFederationController
/// </summary>
public static class ModuleFederationRouting
{
    private const string BaseRouting = "api/module-federation";

    /// GetManifest Route
    public const string GetManifestRoute = $"{BaseRouting}/get-manifest";

    /// RegisterModule Route
    public const string RegisterModuleRoute = $"{BaseRouting}/register-module";
}