using Microsoft.AspNetCore.Mvc;
using Oip.Controllers.Api;

namespace Oip.Controllers;

/// <summary>
/// Module federation controller
/// </summary>
[ApiController]
[Route("api/module-federation")]
public class ModuleFederationController : ControllerBase
{
    private static readonly Dictionary<string, ModuleFederation> Modules = new()
    {
        {
            "mfe1", new ModuleFederation()
            {
                RemoteEntry = "http://localhost:50001/remoteEntry.js",
                BaseUrl = "http://localhost:50001/",
                ExposedModule = "./Module",
                DisplayName = "Flights",
                RoutePath = "flights",
                NgModuleName = "FlightsModule"
            }
        }
    };

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
    public Dictionary<string, ModuleFederation> GetManifest()
    {
        return Modules;
    }

    /// <summary>
    /// Registry module
    /// </summary>
    /// <param name="module"></param>
    /// <param name="moduleFederation"></param>
    /// <returns></returns>
    [HttpPost("register-module")]
    public IActionResult RegisterModule(string module, ModuleFederation moduleFederation)
    {
        _logger.LogInformation("Register module: {module}", module);
        if (Modules.ContainsKey(module))
            Modules.Remove(module);

        Modules.Add(module, moduleFederation);

        return Ok();
    }
}