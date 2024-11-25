using System.Net;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Api;

namespace Oip.Controllers;

/// <summary>
/// Module federation controller
/// </summary>
[ApiController]
[Route("api/module-federation")]
public class ModuleFederationController : ControllerBase
{
    private static readonly Dictionary<string, GetManifestResponse> Modules = new();

    private readonly ILogger<ModuleFederationController> _logger;

    /// <summary>.ctor</summary>
    public ModuleFederationController(ILogger<ModuleFederationController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get manifest for client app
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-manifest")]
    public Dictionary<string, GetManifestResponse> GetManifest()
    {
        return Modules;
    }

    /// <summary>
    /// Registry module
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("register-module")]
    public async Task<IActionResult> RegisterModule(RegisterModuleDto request)
    {
        _logger.LogInformation("Trying register module: {Module}", request.Name);

        using HttpClient client = new HttpClient();
        var remoteEntryResponseTask = client.GetAsync(request.RemoteEntry);
        var baseUrlResponseTask = client.GetAsync(request.BaseUrl);

        Task.WaitAll(remoteEntryResponseTask, baseUrlResponseTask);
        if ((await remoteEntryResponseTask).StatusCode != HttpStatusCode.OK)
            return BadRequest(new InvalidOperationException($"RemoteEntry: {request.RemoteEntry} - unavailable"));
        if ((await baseUrlResponseTask).StatusCode != HttpStatusCode.OK)
            return BadRequest(new InvalidOperationException($"BaseUrl: {request.BaseUrl} - unavailable"));

        Modules.Remove(request.Name);

        foreach (var module in request.ExportModules)
        {
            var moduleFullName = $"{request.Name}::{module.NgModuleName}";
            if (!Modules.ContainsKey(moduleFullName))
            {
                Modules.Add(moduleFullName, new GetManifestResponse
                {
                    BaseUrl = request.BaseUrl,
                    RemoteEntry = request.RemoteEntry,
                    DisplayName = module.DisplayName,
                    ExposedModule = module.ExposedModule,
                    RoutePath = module.RoutePath,
                    NgModuleName = module.NgModuleName
                });
            }
        }

        _logger.LogInformation("Module registered: {Module}", request.Name);

        return Ok();
    }
}