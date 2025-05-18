using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Oip.Base.Api;

namespace Oip.Base.Controllers;

/// <summary>
/// Module federation controller
/// </summary>
[ApiController]
[Route("api/service")]
public class ServiceController : ControllerBase
{
    private static readonly ConcurrentDictionary<string, GetManifestResponse> Modules = new();

    private readonly ILogger<ServiceController> _logger;

    /// <summary>.ctor</summary>
    public ServiceController(ILogger<ServiceController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get manifest for client app
    /// </summary>
    /// <returns></returns>
    [HttpGet("get")]
    public Dictionary<string, GetManifestResponse> Get()
    {
        return Modules.ToDictionary();
    }

    /// <summary>
    /// Registry module
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("register-module")]
    public async Task<IActionResult> RegisterModule(RegisterModuleDto request)
    {
        _logger.LogDebug("Trying register module: {Module}", request.Name);

        if (Modules.ContainsKey(request.Name))
            return Ok();

        using HttpClient client = new HttpClient();
        var baseUrlResponseTask = await client.GetAsync(request.BaseUrl);

        if (baseUrlResponseTask.StatusCode != HttpStatusCode.OK)
            return BadRequest(new InvalidOperationException($"BaseUrl: {request.BaseUrl} - unavailable"));

        Modules.TryAdd(request.Name, new GetManifestResponse
            {
                BaseUrl = request.BaseUrl,
            }
        );

        _logger.LogDebug("Module registered: {Module}", request.Name);

        return Ok();
    }
}