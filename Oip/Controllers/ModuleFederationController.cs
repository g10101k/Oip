using Microsoft.AspNetCore.Mvc;

namespace Oip.Controllers;

[ApiController]
[Route("api/module-federation")]
public class ModuleFederationController : ControllerBase
{
    private static Dictionary<string, ModuleFederation> _modules = new()
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

    public ModuleFederationController(ILogger<ModuleFederationController> logger)
    {
        _logger = logger;
    }

    [HttpGet("get-manifest")]
    public Dictionary<string, ModuleFederation> GetManifest()
    {
        
        return _modules;
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
        if (_modules.ContainsKey(module))
            _modules.Remove(module);

        _modules.Add(module, moduleFederation);

        return Ok();
    }
}