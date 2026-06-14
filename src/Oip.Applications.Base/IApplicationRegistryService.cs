using Oip.Applications.Base.Contracts;

namespace Oip.Applications.Base;

/// <summary>
/// Shared application registry contract used by local and remote implementations.
/// </summary>
public interface IApplicationRegistryService
{
    /// <summary>
    /// Registers or updates an application by its stable code.
    /// </summary>
    Task<ApplicationRegistryItemDto> RegisterApplicationAsync(
        ApplicationRegistryItemDto application,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves registered applications.
    /// </summary>
    Task<IReadOnlyList<ApplicationRegistryItemDto>> GetApplicationRegistryItemsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves frontend module manifests available to the current user.
    /// </summary>
    Task<IReadOnlyList<FrontendRemoteManifestDto>> GetFrontendModuleManifestsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves one frontend module manifest by code.
    /// </summary>
    Task<FrontendRemoteManifestDto> GetFrontendModuleManifestByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves one application by code.
    /// </summary>
    Task<ApplicationRegistryItemDto> GetApplicationRegistryItemByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new application registry item.
    /// </summary>
    Task<ApplicationRegistryItemDto> CreateApplicationRegistryItemAsync(
        ApplicationRegistryItemDto application,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing application registry item identified by code.
    /// </summary>
    Task<ApplicationRegistryItemDto> UpdateApplicationRegistryItemAsync(
        string code,
        ApplicationRegistryItemDto application,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an application registry item by code.
    /// </summary>
    Task DeleteApplicationRegistryItemAsync(string code, CancellationToken cancellationToken = default);
}
