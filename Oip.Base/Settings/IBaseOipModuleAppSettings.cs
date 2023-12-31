using Oip.Settings;

namespace Oip.Base.Settings;

/// <summary>
/// Base settings for module application
/// </summary>
public interface IBaseOipModuleAppSettings : IAppSettings
{
    /// <summary>
    /// Main service with shell
    /// </summary>
    string OipUrls { get; set; }

    /// <summary>
    /// OpenAPI section
    /// </summary>
    OpenApiSettings OpenApi { get; set; }
}