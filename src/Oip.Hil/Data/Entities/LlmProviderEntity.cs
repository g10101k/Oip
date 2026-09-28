using Oip.Hil.Services;

namespace Oip.Hil.Data.Entities;

/// <summary>
/// Stored LLM provider configuration.
/// </summary>
public class LlmProviderEntity
{
    /// <summary>
    /// Primary key.
    /// </summary>
    public int LlmProviderId { get; set; }

    /// <summary>
    /// Human-readable name shown in the UI.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Provider API kind.
    /// </summary>
    public LlmProviderType ProviderType { get; set; } = LlmProviderType.OpenAi;

    /// <summary>
    /// API base URL, e.g. https://api.openai.com/v1.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Default model identifier, e.g. gpt-4o-mini.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// API key protected with ASP.NET Core Data Protection. Null when no key is configured.
    /// </summary>
    public string? ApiKeyProtected { get; set; }

    /// <summary>
    /// Whether this provider is used when no provider is specified explicitly. At most one provider is default.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Whether the provider can be used.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
