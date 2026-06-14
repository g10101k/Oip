using Oip.Base.Settings;

namespace Oip.Applications.Base.Contracts;

/// <summary>
/// Frontend application registry item.
/// </summary>
public class ApplicationRegistryItemDto
{
    /// <summary>
    /// Stable application code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable application name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Frontend application URL.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Internal base URL.
    /// </summary>
    public string InternalBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// PrimeIcons CSS class.
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Display order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Indicates whether application should be returned to frontend.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Type of service.
    /// </summary>
    public ServiceType ServiceType { get; set; } = ServiceType.Service;

    /// <summary>
    /// Indicates whether this application is current.
    /// </summary>
    public bool IsCurrent { get; set; }
}

/// <summary>
/// Describes how the shell integrates a frontend module.
/// </summary>
public enum FrontendIntegrationType
{
    /// <summary>
    /// Module is implemented by a route inside the shell application.
    /// </summary>
    InternalRoute = 0,

    /// <summary>
    /// Module is rendered through an iframe.
    /// </summary>
    Iframe = 1,

    /// <summary>
    /// Module is loaded as a trusted module federation remote.
    /// </summary>
    FederatedRemote = 2,

    /// <summary>
    /// Module is exposed as a web component.
    /// </summary>
    WebComponent = 3
}

/// <summary>
/// Describes the exported entrypoint shape of a frontend remote.
/// </summary>
public enum FrontendRemoteEntryKind
{
    /// <summary>
    /// Remote exports Angular routes.
    /// </summary>
    Routes = 0,

    /// <summary>
    /// Remote exports a component.
    /// </summary>
    Component = 1
}

/// <summary>
/// Known shell fallback states for remote frontend modules.
/// </summary>
public enum FrontendModuleErrorState
{
    /// <summary>
    /// Manifest data is invalid or incomplete.
    /// </summary>
    InvalidManifest = 0,

    /// <summary>
    /// Remote entrypoint cannot be reached.
    /// </summary>
    RemoteUnavailable = 1,

    /// <summary>
    /// Remote requirements are incompatible with the current shell.
    /// </summary>
    IncompatibleVersion = 2,

    /// <summary>
    /// Remote loaded but did not export the expected contract.
    /// </summary>
    InvalidRemoteExport = 3,

    /// <summary>
    /// User does not have permission to open the module.
    /// </summary>
    PermissionDenied = 4,

    /// <summary>
    /// Remote failed at runtime after it was loaded.
    /// </summary>
    RuntimeError = 5
}

/// <summary>
/// Frontend module manifest consumed by the shell.
/// </summary>
public class FrontendRemoteManifestDto
{
    /// <summary>
    /// Stable frontend module code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable module title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Frontend integration mode.
    /// </summary>
    public FrontendIntegrationType Type { get; set; } = FrontendIntegrationType.InternalRoute;

    /// <summary>
    /// Route path owned by the shell for this module.
    /// </summary>
    public string RoutePath { get; set; } = string.Empty;

    /// <summary>
    /// Module federation remote entry URL.
    /// </summary>
    public string? RemoteEntryUrl { get; set; }

    /// <summary>
    /// Module federation remote name.
    /// </summary>
    public string? RemoteName { get; set; }

    /// <summary>
    /// Exposed module name, for example ./Routes.
    /// </summary>
    public string? ExposedModule { get; set; }

    /// <summary>
    /// Remote entrypoint kind.
    /// </summary>
    public FrontendRemoteEntryKind EntryKind { get; set; } = FrontendRemoteEntryKind.Routes;

    /// <summary>
    /// Semver range required for the OIP shell.
    /// </summary>
    public string? RequiredShellVersion { get; set; }

    /// <summary>
    /// Semver range required for oip-common.
    /// </summary>
    public string? RequiredOipCommonVersion { get; set; }

    /// <summary>
    /// Semver range required for Angular.
    /// </summary>
    public string? AngularVersion { get; set; }

    /// <summary>
    /// Permissions required to display and load the module.
    /// </summary>
    public IReadOnlyList<string> Permissions { get; set; } = [];

    /// <summary>
    /// Indicates whether the module manifest is active.
    /// </summary>
    public bool Enabled { get; set; } = true;
}
