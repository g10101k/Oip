using System.Net.Http.Json;
using Oip.Base.Api;

namespace Oip.Base.Services;

/// <summary>
/// HTTP Client 
/// </summary>
public class ModuleFederationService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// .ctor
    /// </summary>
    public ModuleFederationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Register module
    /// </summary>
    /// <param name="request"></param>
    public async Task RegisterModuleAsync(RegisterModuleDto request)
    {
        await _httpClient.PostAsync(ModuleFederationRouting.RegisterModuleRoute, JsonContent.Create(request));
    }
}