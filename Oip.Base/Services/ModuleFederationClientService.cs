using System.Net;
using System.Net.Http.Json;
using Oip.Base.Api;

namespace Oip.Base.Services;

/// <summary>
/// HTTP Client 
/// </summary>
public class ModuleFederationClientService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// .ctor
    /// </summary>
    public ModuleFederationClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Register module
    /// </summary>
    /// <param name="module"></param>
    public async Task RegisterModuleAsync(RegisterModuleDto module)
    {
        await _httpClient.PostAsync(ModuleFederationRouting.RegisterModuleRoute, JsonContent.Create(module));
    }
}