using System.Text.Json.Nodes;

namespace Oip.Data.Dtos;

/// <summary>
/// Manifest supplied by an external Web Component extension.
/// </summary>
public class ExtensionModuleManifestDto
{
    /// <summary>
    /// Module Federation extension loader.
    /// </summary>
    public const string ModuleFederationLoadType = "moduleFederation";

    /// <summary>
    /// Custom Element extension loader.
    /// </summary>
    public const string CustomElementLoadType = "customElement";

    /// <summary>
    /// Stable extension key.
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Extension version.
    /// </summary>
    public string Version { get; set; } = null!;

    /// <summary>
    /// Route path used inside OIP.
    /// </summary>
    public string RoutePath { get; set; } = null!;

    /// <summary>
    /// Extension loader type.
    /// </summary>
    public string LoadType { get; set; } = CustomElementLoadType;

    /// <summary>
    /// Custom element tag name.
    /// </summary>
    public string ElementName { get; set; } = null!;

    /// <summary>
    /// JavaScript bundle URL that registers the custom element.
    /// </summary>
    public string ScriptUrl { get; set; } = null!;

    /// <summary>
    /// Module Federation remote entry URL.
    /// </summary>
    public string? RemoteEntryUrl { get; set; }

    /// <summary>
    /// Module Federation exposed module name.
    /// </summary>
    public string? ExposedModule { get; set; }

    /// <summary>
    /// Exported Angular component name.
    /// </summary>
    public string? ComponentName { get; set; }

    /// <summary>
    /// Extension backend API base URL.
    /// </summary>
    public string ApiBaseUrl { get; set; } = null!;

    /// <summary>
    /// Optional PrimeIcons icon name.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional permission descriptors.
    /// </summary>
    public JsonNode? Permissions { get; set; }

    /// <summary>
    /// Optional settings schema.
    /// </summary>
    public JsonNode? SettingsSchema { get; set; }
}
