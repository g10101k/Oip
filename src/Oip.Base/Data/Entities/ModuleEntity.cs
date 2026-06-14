namespace Oip.Base.Data.Entities;

/// <summary>
/// Module entity
/// </summary>
public class ModuleEntity
{
    /// <summary>
    /// Id
    /// </summary>
    public int ModuleId { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Settings for module
    /// </summary>
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? Settings { get; set; }
    
    /// <summary>
    /// Route link to component
    /// </summary>
    public string? RouterLink { get; set; }

    /// <summary>
    /// Module delivery kind.
    /// </summary>
    public ModuleKind Kind { get; set; } = ModuleKind.Core;

    /// <summary>
    /// URL of the extension manifest, if this module is an extension.
    /// </summary>
    public string? ManifestUrl { get; set; }

    /// <summary>
    /// Stable extension key.
    /// </summary>
    public string? ExtensionKey { get; set; }

    /// <summary>
    /// Extension loader type.
    /// </summary>
    public string? LoadType { get; set; }

    /// <summary>
    /// Custom element tag name exposed by the extension bundle.
    /// </summary>
    public string? ElementName { get; set; }

    /// <summary>
    /// JavaScript entrypoint that registers the custom element.
    /// </summary>
    public string? ScriptUrl { get; set; }

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
    /// Backend API base URL for the extension service.
    /// </summary>
    public string? ApiBaseUrl { get; set; }

    /// <summary>
    /// Extension version.
    /// </summary>
    public string? Version { get; set; }
    
    /// <summary>
    /// Module Securities
    /// </summary>
    public ICollection<ModuleSecurityEntity> ModuleSecurities { get; set; } = new List<ModuleSecurityEntity>();
    
    /// <summary>
    /// Instances
    /// </summary>
    public ICollection<ModuleInstanceEntity> ModuleInstances { get; set; } = new List<ModuleInstanceEntity>();
}
