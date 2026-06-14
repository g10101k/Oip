namespace Oip.Data.Dtos;

/// <summary>
/// Request to register an extension module from a manifest URL.
/// </summary>
public record RegisterExtensionModuleRequest(string ManifestUrl);

/// <summary>
/// Request to update an already registered extension module from a manifest URL.
/// </summary>
public record UpdateExtensionModuleRequest(string ManifestUrl);
