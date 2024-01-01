using Microsoft.AspNetCore.Mvc;
using Oip.Base.Api;

namespace Oip.Controllers;

/// <summary>
/// Module federation controller
/// </summary>
[ApiController]
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
    [HttpGet(ModuleFederationRouting.GetManifestRoute)]
    public Dictionary<string, ModuleFederationDto> GetManifest()
    {
        return Modules;
    }

    /// <summary>
    /// Registry module
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost(ModuleFederationRouting.RegisterModuleRoute)]
    public IActionResult RegisterModule(RegisterModuleDto request)
    {
        _logger.LogInformation("Register module: {module}", request.Name);
        if (Modules.ContainsKey(request.Name))
            Modules.Remove(request.Name);

        Modules.Add(request.Name, request.ModuleFederationDto);

        return Ok();
    }
}