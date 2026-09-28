using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Oip.Hil.AiFunctions;
using Oip.Hil.Controllers.Api;
using Oip.Hil.Data.Contexts;
using Oip.Hil.Data.Entities;
using Oip.Base.Exceptions;
using OpenAI;
using OpenAI.Chat;

namespace Oip.Hil.Services;

/// <summary>
/// CRUD, connectivity checks and agent runs (Microsoft Agent Framework) for LLM providers. API keys are encrypted
/// at rest with Data Protection.
/// </summary>
public class LlmProviderService(
    LlmContext context,
    IDataProtectionProvider dataProtectionProvider,
    IHttpClientFactory httpClientFactory,
    LlmProviderTools providerTools,
    ILogger<LlmProviderService> logger)
{
    private const string ProtectorPurpose = "Oip.LlmProviders.ApiKey";
    private static readonly TimeSpan ChatTimeout = TimeSpan.FromSeconds(120);
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);

    /// <summary>
    /// Returns all providers ordered by default flag and name.
    /// </summary>
    public async Task<List<LlmProviderDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var providers = await context.Providers
            .AsNoTracking()
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return providers.Select(ToDto).ToList();
    }

    /// <summary>
    /// Creates a provider.
    /// </summary>
    public async Task<LlmProviderDto> CreateAsync(SaveLlmProviderRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        await EnsureNameIsUniqueAsync(request.Name, null, cancellationToken);

        var now = DateTime.UtcNow;
        var provider = new LlmProviderEntity
        {
            CreatedAt = now
        };
        Apply(provider, request, now);

        if (string.IsNullOrWhiteSpace(request.ApiKey))
            throw new ApiException("Validation error", "API key is required for a new provider", StatusCodes.Status400BadRequest);

        provider.ApiKeyProtected = _protector.Protect(request.ApiKey.Trim());

        if (provider.IsDefault)
            await ClearDefaultAsync(null, cancellationToken);

        context.Providers.Add(provider);
        await context.SaveChangesAsync(cancellationToken);
        return ToDto(provider);
    }

    /// <summary>
    /// Updates a provider. The stored API key is kept when <see cref="SaveLlmProviderRequest.ApiKey"/> is empty.
    /// </summary>
    public async Task<LlmProviderDto> UpdateAsync(int id, SaveLlmProviderRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var provider = await FindAsync(id, cancellationToken);
        await EnsureNameIsUniqueAsync(request.Name, id, cancellationToken);

        Apply(provider, request, DateTime.UtcNow);

        if (!string.IsNullOrWhiteSpace(request.ApiKey))
            provider.ApiKeyProtected = _protector.Protect(request.ApiKey.Trim());

        if (provider.IsDefault)
            await ClearDefaultAsync(id, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return ToDto(provider);
    }

    /// <summary>
    /// Deletes a provider.
    /// </summary>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var provider = await FindAsync(id, cancellationToken);
        context.Providers.Remove(provider);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Calls <c>GET {BaseUrl}/models</c> with the stored API key and reports the outcome.
    /// </summary>
    public async Task<TestLlmProviderResponse> TestAsync(int id, CancellationToken cancellationToken)
    {
        var provider = await FindAsync(id, cancellationToken);
        var apiKey = UnprotectApiKey(provider);
        var stopwatch = Stopwatch.StartNew();
        var result = await ListModelsAsync(provider.BaseUrl, apiKey, cancellationToken);
        stopwatch.Stop();

        if (result.Error is not null)
        {
            return new TestLlmProviderResponse(false, result.Error, stopwatch.ElapsedMilliseconds);
        }

        return new TestLlmProviderResponse(
            true,
            $"OK, {result.Models.Count} model(s) available",
            stopwatch.ElapsedMilliseconds,
            result.Models.Count,
            result.Models.Contains(provider.Model, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Lists model identifiers available at the given endpoint. Uses the key from the request, or the stored key of
    /// <see cref="GetLlmProviderModelsRequest.ProviderId"/> when the request key is empty.
    /// </summary>
    public async Task<List<string>> GetModelsAsync(GetLlmProviderModelsRequest request, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(request.BaseUrl?.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ApiException("Validation error", "Base URL must be an absolute http(s) URL", StatusCodes.Status400BadRequest);

        var apiKey = request.ApiKey?.Trim();
        if (string.IsNullOrEmpty(apiKey) && request.ProviderId is { } providerId)
            apiKey = UnprotectApiKey(await FindAsync(providerId, cancellationToken));

        var result = await ListModelsAsync(request.BaseUrl!.Trim(), apiKey, cancellationToken);
        if (result.Error is not null)
            throw new ApiException("Provider request failed", result.Error, StatusCodes.Status400BadRequest);

        return result.Models;
    }

    /// <summary>
    /// Runs an Agent Framework agent over the chat completions API of the requested or the default provider.
    /// When <see cref="LlmRequest.Outcomes"/> are set, the agent is required to call the
    /// <see cref="LlmProviderTools.SelectOutcome"/> tool and the selected outcome is returned in
    /// <see cref="LlmResponse.Outcome"/>.
    /// </summary>
    /// <exception cref="ApiException">The provider is not found, disabled or not configured.</exception>
    /// <exception cref="LlmProviderException">The provider rejected the request or no outcome was selected.</exception>
    public async Task<LlmResponse> CompleteChatAsync(LlmRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            throw new ApiException("Validation error", "Prompt is required", StatusCodes.Status400BadRequest);

        var provider = await FindChatProviderAsync(request.ProviderId, cancellationToken);
        var model = string.IsNullOrWhiteSpace(request.Model) ? provider.Model : request.Model.Trim();
        var outcomes = request.Outcomes?.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList() ?? [];
        var tools = providerTools.AsAiTools(outcomes);

        var agent = CreateChatClient(provider, model).AsAIAgent(
            instructions: string.IsNullOrWhiteSpace(request.SystemPrompt) ? null : request.SystemPrompt,
            tools: tools);
        var settings = ValidateSettings(request.Settings);
        var options = new ChatClientAgentRunOptions(new ChatOptions
        {
            ToolMode = outcomes.Count == 0 ? null : ChatToolMode.RequireSpecific(LlmProviderTools.SelectOutcomeToolName),
            RawRepresentationFactory = settings.Count == 0 ? null : _ => CreateCompletionOptions(settings)
        });

        var stopwatch = Stopwatch.StartNew();
        AgentResponse response;
        try
        {
            response = await agent.RunAsync(request.Prompt, options: options, cancellationToken: cancellationToken);
        }
        catch (ClientResultException e) when (e.Status is not (0 or StatusCodes.Status408RequestTimeout
                                                  or StatusCodes.Status429TooManyRequests) && e.Status < 500)
        {
            throw new LlmProviderException(e.Message, false);
        }

        stopwatch.Stop();

        if (outcomes.Count > 0 && providerTools.SelectedOutcome is null)
            throw new LlmProviderException(
                $"The model did not select an outcome with {LlmProviderTools.SelectOutcomeToolName}, expected one of: " +
                string.Join(", ", outcomes), true);

        return new LlmResponse(
            outcomes.Count == 0 ? response.Text : providerTools.SelectedReason,
            outcomes.Count == 0 ? null : providerTools.SelectedOutcome,
            provider.Name,
            model,
            response.Usage?.InputTokenCount,
            response.Usage?.OutputTokenCount,
            stopwatch.ElapsedMilliseconds);
    }

    private static IReadOnlyDictionary<string, JsonElement> ValidateSettings(
        IReadOnlyDictionary<string, JsonElement>? settings)
    {
        if (settings is null) return new Dictionary<string, JsonElement>();

        // Keys become JSON paths of the request body, so only plain property names are accepted.
        var invalid = settings.Keys.Where(key => key.Length == 0 || !key.All(c => char.IsAsciiLetterOrDigit(c) || c == '_'))
            .ToList();
        if (invalid.Count > 0)
            throw new ApiException("Validation error",
                $"Invalid LLM request settings: {string.Join(", ", invalid)}. Use names like top_p or max_tokens",
                StatusCodes.Status400BadRequest);
        return settings;
    }

    /// <summary>
    /// Adds the settings to the chat completions request body as is; the agent fills in the messages, tools and
    /// the other options on top of it.
    /// </summary>
    private static ChatCompletionOptions CreateCompletionOptions(IReadOnlyDictionary<string, JsonElement> settings)
    {
        var options = new ChatCompletionOptions();
#pragma warning disable SCME0001 // JsonPatch is experimental; it is the SDK way to send parameters it has no properties for.
        foreach (var (key, value) in settings)
            options.Patch.Set(Encoding.UTF8.GetBytes("$." + key), BinaryData.FromString(value.GetRawText()));
#pragma warning restore SCME0001
        return options;
    }

    private ChatClient CreateChatClient(LlmProviderEntity provider, string model)
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.Timeout = ChatTimeout;
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(provider.BaseUrl),
            NetworkTimeout = ChatTimeout,
            // Retries are done by the Temporal activity.
            RetryPolicy = new ClientRetryPolicy(0),
            Transport = new HttpClientPipelineTransport(httpClient)
        };
        // Local OpenAI-compatible servers accept any key, but the SDK requires one.
        return new ChatClient(model, new ApiKeyCredential(UnprotectApiKey(provider) ?? "none"), options);
    }

    private async Task<LlmProviderEntity> FindChatProviderAsync(int? providerId, CancellationToken cancellationToken)
    {
        var provider = providerId is { } id
            ? await FindAsync(id, cancellationToken)
            : await context.Providers.AsNoTracking().FirstOrDefaultAsync(x => x.IsDefault && x.IsEnabled, cancellationToken)
              ?? throw new ApiException("Validation error", "No enabled default LLM provider is configured",
                  StatusCodes.Status400BadRequest);

        if (!provider.IsEnabled)
            throw new ApiException("Validation error", $"LLM provider '{provider.Name}' is disabled",
                StatusCodes.Status400BadRequest);
        return provider;
    }

    private sealed record ListModelsResult(List<string> Models, string? Error);

    private async Task<ListModelsResult> ListModelsAsync(string baseUrl, string? apiKey, CancellationToken cancellationToken)
    {
        try
        {
            using var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            using var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl(baseUrl, "models"));
            if (!string.IsNullOrEmpty(apiKey))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                return new ListModelsResult([], $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {ExtractErrorMessage(body)}");

            var models = ParseModelIds(body);
            models.Sort(StringComparer.OrdinalIgnoreCase);
            return new ListModelsResult(models, null);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(e, "LLM provider request to {BaseUrl} failed", baseUrl);
            return new ListModelsResult([], e.Message);
        }
    }

    private string? UnprotectApiKey(LlmProviderEntity providerEntity) =>
        providerEntity.ApiKeyProtected is null ? null : _protector.Unprotect(providerEntity.ApiKeyProtected);

    private async Task<LlmProviderEntity> FindAsync(int id, CancellationToken cancellationToken)
    {
        return await context.Providers.FirstOrDefaultAsync(x => x.LlmProviderId == id, cancellationToken)
               ?? throw new ApiException("Not found", $"LLM provider {id} not found", StatusCodes.Status404NotFound);
    }

    private async Task EnsureNameIsUniqueAsync(string name, int? exceptId, CancellationToken cancellationToken)
    {
        var normalized = name.Trim();
        var exists = await context.Providers.AnyAsync(x => x.Name == normalized && x.LlmProviderId != exceptId, cancellationToken);
        if (exists)
            throw new ApiException("Validation error", $"Provider with name '{normalized}' already exists", StatusCodes.Status400BadRequest);
    }

    private async Task ClearDefaultAsync(int? exceptId, CancellationToken cancellationToken)
    {
        await context.Providers
            .Where(x => x.IsDefault && x.LlmProviderId != exceptId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsDefault, false), cancellationToken);
    }

    private static void Validate(SaveLlmProviderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ApiException("Validation error", "Name is required", StatusCodes.Status400BadRequest);
        if (string.IsNullOrWhiteSpace(request.Model))
            throw new ApiException("Validation error", "Model is required", StatusCodes.Status400BadRequest);
        if (!Uri.TryCreate(request.BaseUrl?.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ApiException("Validation error", "Base URL must be an absolute http(s) URL", StatusCodes.Status400BadRequest);
    }

    private static void Apply(LlmProviderEntity providerEntity, SaveLlmProviderRequest request, DateTime now)
    {
        providerEntity.Name = request.Name.Trim();
        providerEntity.ProviderType = request.ProviderType;
        providerEntity.BaseUrl = request.BaseUrl.Trim().TrimEnd('/');
        providerEntity.Model = request.Model.Trim();
        providerEntity.IsDefault = request.IsDefault;
        providerEntity.IsEnabled = request.IsEnabled;
        providerEntity.UpdatedAt = now;
    }

    private LlmProviderDto ToDto(LlmProviderEntity providerEntity)
    {
        string? hint = null;
        if (UnprotectApiKey(providerEntity) is { } key)
            hint = key.Length > 4 ? $"••••••••{key[^4..]}" : "••••••••";

        return new LlmProviderDto(
            providerEntity.LlmProviderId,
            providerEntity.Name,
            providerEntity.ProviderType,
            providerEntity.BaseUrl,
            providerEntity.Model,
            providerEntity.ApiKeyProtected is not null,
            hint,
            providerEntity.IsDefault,
            providerEntity.IsEnabled,
            providerEntity.CreatedAt,
            providerEntity.UpdatedAt);
    }

    private static string BuildUrl(string baseUrl, string path) => $"{baseUrl.TrimEnd('/')}/{path}";

    private static List<string> ParseModelIds(string body)
    {
        using var document = JsonDocument.Parse(body);
        if (!document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
            return [];

        return data.EnumerateArray()
            .Select(x => x.TryGetProperty("id", out var idElement) ? idElement.GetString() : null)
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => x!)
            .ToList();
    }

    private static string ExtractErrorMessage(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("error", out var error))
            {
                if (error.ValueKind == JsonValueKind.Object && error.TryGetProperty("message", out var message))
                    return message.GetString() ?? body;
                if (error.ValueKind == JsonValueKind.String)
                    return error.GetString() ?? body;
            }
        }
        catch (JsonException)
        {
            // not JSON, fall through
        }

        return body.Length > 300 ? body[..300] : body;
    }
}
