using Oip.Hil.Services;

namespace Oip.Hil.Controllers.Api;

/// <summary>
/// Settings for the LlmProvider module instance.
/// </summary>
public class LlmProviderModuleSettings
{
    /// <summary>
    /// Show disabled providers in the list.
    /// </summary>
    public bool ShowDisabled { get; set; } = true;
}

/// <summary>
/// LLM provider as exposed to the client. The API key itself is never returned.
/// </summary>
/// <param name="Id">Primary key.</param>
/// <param name="Name">Display name.</param>
/// <param name="ProviderType">Provider API kind.</param>
/// <param name="BaseUrl">API base URL.</param>
/// <param name="Model">Default model identifier.</param>
/// <param name="HasApiKey">Whether an API key is stored.</param>
/// <param name="ApiKeyHint">Masked API key, e.g. "••••••••abcd". Null when no key is stored.</param>
/// <param name="IsDefault">Whether this is the default provider.</param>
/// <param name="IsEnabled">Whether the provider is enabled.</param>
/// <param name="CreatedAt">Creation timestamp (UTC).</param>
/// <param name="UpdatedAt">Last update timestamp (UTC).</param>
public record LlmProviderDto(
    int Id,
    string Name,
    LlmProviderType ProviderType,
    string BaseUrl,
    string Model,
    bool HasApiKey,
    string? ApiKeyHint,
    bool IsDefault,
    bool IsEnabled,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// Create or update request for an LLM provider.
/// </summary>
/// <param name="Name">Display name (unique).</param>
/// <param name="BaseUrl">API base URL, e.g. https://api.openai.com/v1.</param>
/// <param name="Model">Default model identifier.</param>
/// <param name="ApiKey">New API key. Leave null or empty on update to keep the stored key.</param>
/// <param name="ProviderType">Provider API kind.</param>
/// <param name="IsDefault">Whether this provider becomes the default one.</param>
/// <param name="IsEnabled">Whether the provider is enabled.</param>
public record SaveLlmProviderRequest(
    string Name,
    string BaseUrl,
    string Model,
    string? ApiKey = null,
    LlmProviderType ProviderType = LlmProviderType.OpenAi,
    bool IsDefault = false,
    bool IsEnabled = true);

/// <summary>
/// Request to list models available at a provider endpoint.
/// </summary>
/// <param name="BaseUrl">API base URL, e.g. https://api.openai.com/v1.</param>
/// <param name="ApiKey">API key to use. Empty means "use the key stored for <paramref name="ProviderId"/>".</param>
/// <param name="ProviderId">Stored provider to take the API key from when <paramref name="ApiKey"/> is empty. Null for a provider that is not saved yet.</param>
public record GetLlmProviderModelsRequest(string BaseUrl, string? ApiKey = null, int? ProviderId = null);

/// <summary>
/// Result of a connectivity check against the provider API.
/// </summary>
/// <param name="Success">Whether the API answered successfully.</param>
/// <param name="Message">Human-readable outcome.</param>
/// <param name="ElapsedMs">Round-trip time in milliseconds.</param>
/// <param name="ModelsCount">Number of models the API reported. Null when the call failed.</param>
/// <param name="ModelFound">Whether the configured model was found in the list. Null when the call failed.</param>
public record TestLlmProviderResponse(
    bool Success,
    string Message,
    long ElapsedMs,
    int? ModelsCount = null,
    bool? ModelFound = null);
