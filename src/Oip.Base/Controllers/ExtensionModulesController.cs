using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Dtos;
using Oip.Base.Data.Entities;
using Oip.Base.Data.Repositories;
using Oip.Base.Exceptions;

namespace Oip.Base.Controllers;

/// <summary>
/// API controller for managing runtime extension modules.
/// </summary>
[ApiController]
[Route("api/extension-modules")]
[ApiExplorerSettings(GroupName = "base")]
public class ExtensionModulesController(ModuleRepository moduleRepository, IHttpClientFactory httpClientFactory)
    : ControllerBase
{
    private const string SupportedHostVersion = "1";
    private static readonly Regex ElementNameRegex =
        new("^[a-z][a-z0-9]*(-[a-z0-9]+)+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Retrieves all registered extension modules.
    /// </summary>
    [HttpGet("get-extension-modules")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType<IEnumerable<ModuleDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public async Task<IEnumerable<ModuleDto>> GetExtensionModules()
    {
        return (await moduleRepository.GetAll()).Where(x => x.Kind == ModuleKind.Extension);
    }

    /// <summary>
    /// Retrieves extension metadata by stable key.
    /// </summary>
    [HttpGet("get-extension-module-by-key/{extensionKey}")]
    [Authorize]
    [ProducesResponseType<ModuleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<ModuleDto> GetExtensionModuleByKey(string extensionKey)
    {
        return await moduleRepository.GetExtensionModuleByKey(extensionKey)
               ?? throw new ApiException(
                   "Extension module not found",
                   $"Extension module with key '{extensionKey}' was not found.",
                   StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Registers an extension module from a manifest URL.
    /// </summary>
    [HttpPost("register-extension-module")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType<ModuleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public async Task<ModuleDto> RegisterExtensionModule(RegisterExtensionModuleRequest request)
    {
        var manifest = await LoadAndValidateManifest(request.ManifestUrl);

        try
        {
            return await moduleRepository.RegisterExtensionModule(manifest, request.ManifestUrl);
        }
        catch (InvalidOperationException ex)
        {
            throw new ApiException("Extension module registration failed", ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>
    /// Updates an extension module from a manifest URL.
    /// </summary>
    [HttpPut("update-extension-module/{id}")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType<ModuleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<ModuleDto> UpdateExtensionModule(int id, UpdateExtensionModuleRequest request)
    {
        var manifest = await LoadAndValidateManifest(request.ManifestUrl);

        try
        {
            return await moduleRepository.UpdateExtensionModule(id, manifest, request.ManifestUrl);
        }
        catch (KeyNotFoundException ex)
        {
            throw new ApiException("Extension module not found", ex.Message, StatusCodes.Status404NotFound);
        }
        catch (InvalidOperationException ex)
        {
            throw new ApiException("Extension module update failed", ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>
    /// Deletes an extension module.
    /// </summary>
    [HttpDelete("delete-extension-module/{id}")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task DeleteExtensionModule(int id)
    {
        try
        {
            await moduleRepository.DeleteExtensionModule(id);
        }
        catch (KeyNotFoundException ex)
        {
            throw new ApiException("Extension module not found", ex.Message, StatusCodes.Status404NotFound);
        }
    }

    private async Task<ExtensionModuleManifestDto> LoadAndValidateManifest(string manifestUrl)
    {
        if (!Uri.TryCreate(manifestUrl, UriKind.Absolute, out var manifestUri) ||
            (manifestUri.Scheme != Uri.UriSchemeHttp && manifestUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ApiException("Invalid extension manifest", "ManifestUrl must be an absolute HTTP or HTTPS URL.",
                StatusCodes.Status400BadRequest);
        }

        var client = httpClientFactory.CreateClient();
        ExtensionModuleManifestDto? manifest;
        try
        {
            await using var stream = await client.GetStreamAsync(manifestUri);
            manifest = await JsonSerializer.DeserializeAsync<ExtensionModuleManifestDto>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or NotSupportedException)
        {
            throw new ApiException("Invalid extension manifest", ex.Message, StatusCodes.Status400BadRequest);
        }

        if (manifest is null)
        {
            throw new ApiException("Invalid extension manifest", "Manifest content is empty.",
                StatusCodes.Status400BadRequest);
        }

        ValidateManifest(manifest, manifestUri);
        return manifest;
    }

    private static void ValidateManifest(ExtensionModuleManifestDto manifest, Uri manifestUri)
    {
        RequireValue(manifest.Key, "key");
        RequireValue(manifest.Name, "name");
        RequireValue(manifest.Version, "version");
        RequireValue(manifest.RoutePath, "routePath");
        RequireValue(manifest.ApiBaseUrl, "apiBaseUrl");

        var loadType = string.IsNullOrWhiteSpace(manifest.LoadType)
            ? ExtensionModuleManifestDto.CustomElementLoadType
            : manifest.LoadType;
        manifest.LoadType = loadType;

        if (loadType is not ExtensionModuleManifestDto.ModuleFederationLoadType and not ExtensionModuleManifestDto.CustomElementLoadType)
        {
            throw new ApiException(
                "Invalid extension manifest",
                $"loadType must be '{ExtensionModuleManifestDto.ModuleFederationLoadType}' or '{ExtensionModuleManifestDto.CustomElementLoadType}'.",
                StatusCodes.Status400BadRequest);
        }

        if (loadType == ExtensionModuleManifestDto.CustomElementLoadType)
        {
            RequireValue(manifest.ElementName, "elementName");
            RequireValue(manifest.ScriptUrl, "scriptUrl");
        }
        else
        {
            RequireValue(manifest.RemoteEntryUrl, "remoteEntryUrl");
            RequireValue(manifest.ExposedModule, "exposedModule");
            RequireValue(manifest.ComponentName, "componentName");
        }

        if (loadType == ExtensionModuleManifestDto.CustomElementLoadType &&
            !ElementNameRegex.IsMatch(manifest.ElementName))
        {
            throw new ApiException(
                "Invalid extension manifest",
                "elementName must be a valid custom element name and contain at least one dash.",
                StatusCodes.Status400BadRequest);
        }

        if (!manifest.Version.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
                ?.Equals(SupportedHostVersion, StringComparison.Ordinal) ?? true)
        {
            throw new ApiException(
                "Invalid extension manifest",
                $"Only extension host major version {SupportedHostVersion} is supported.",
                StatusCodes.Status400BadRequest);
        }

        var apiUri = ParseTrustedUrl(manifest.ApiBaseUrl, nameof(manifest.ApiBaseUrl));
        var entryUri = ParseTrustedUrl(
            loadType == ExtensionModuleManifestDto.ModuleFederationLoadType
                ? manifest.RemoteEntryUrl!
                : manifest.ScriptUrl,
            loadType == ExtensionModuleManifestDto.ModuleFederationLoadType
                ? nameof(manifest.RemoteEntryUrl)
                : nameof(manifest.ScriptUrl));

        if (!IsTrustedOrigin(manifestUri, entryUri) || !IsTrustedOrigin(manifestUri, apiUri))
        {
            throw new ApiException(
                "Untrusted extension origin",
                "remoteEntryUrl/scriptUrl and apiBaseUrl must use the same origin as the manifest.",
                StatusCodes.Status400BadRequest);
        }
    }

    private static Uri ParseTrustedUrl(string value, string fieldName)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ApiException("Invalid extension manifest", $"{fieldName} must be an absolute HTTP or HTTPS URL.",
                StatusCodes.Status400BadRequest);
        }

        return uri;
    }

    private static bool IsTrustedOrigin(Uri manifestUri, Uri candidate)
    {
        return Uri.Compare(
            manifestUri,
            candidate,
            UriComponents.SchemeAndServer,
            UriFormat.Unescaped,
            StringComparison.OrdinalIgnoreCase) == 0;
    }

    private static void RequireValue(string? value, string fieldName)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        throw new ApiException("Invalid extension manifest", $"{fieldName} is required.",
            StatusCodes.Status400BadRequest);
    }
}
