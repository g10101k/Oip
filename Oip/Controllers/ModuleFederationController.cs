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
    private static readonly Dictionary<string, ModuleFederationDto> Modules = new();

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
    public Dictionary<string, ModuleFederationDto> GetManifest()
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
        _logger.LogInformation("Trying register module: {module}", request.Name);

        using HttpClient client = new HttpClient();
        var remoteEntryResponseTask = client.GetAsync(request.ExportModule.RemoteEntry);
        var baseUrlResponseTask = client.GetAsync(request.ExportModule.BaseUrl);

        Task.WaitAll(remoteEntryResponseTask, baseUrlResponseTask);
        if ((await remoteEntryResponseTask).StatusCode != HttpStatusCode.OK)
            return BadRequest(new InvalidOperationException($"RemoteEntry: {request.ExportModule.RemoteEntry} - unavailable"));
        if ((await baseUrlResponseTask).StatusCode != HttpStatusCode.OK)
            return BadRequest(new InvalidOperationException($"BaseUrl: {request.ExportModule.BaseUrl} - unavailable"));

        if (Modules.ContainsKey(request.Name))
            Modules.Remove(request.Name);

        Modules.Add(request.Name, request.ExportModule);
        _logger.LogInformation("Module registered: {module}", request.Name);

        return Ok();
    }
}