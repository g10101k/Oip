using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Oip.Base.Discovery;

namespace Oip.Base.Controllers;

[ApiController]
[Microsoft.AspNetCore.Components.Route("api/service-discovery")]
public sealed class ServiceDiscoveryController(
    IServiceRegistry serviceRegistry,
    ILogger<ServiceDiscoveryController> logger)
    : ControllerBase
{
    private readonly ILogger<ServiceDiscoveryController> _logger = logger;

    [HttpGet("services")]
    public async Task<ActionResult<IReadOnlyList<ServiceInfo>>> GetServices()
    {
        var services = await serviceRegistry.GetActiveServicesAsync();
        return Ok(services);
    }

    [HttpGet("services/{type}")]
    public async Task<ActionResult<IReadOnlyList<ServiceInfo>>> GetServicesByType(string type)
    {
        var services = await serviceRegistry.GetServicesByTypeAsync(type);
        return Ok(services);
    }

    [HttpPost("manual")]
    public async Task<ActionResult> AddManualService([FromBody] ServiceInfo service)
    {
        if (string.IsNullOrEmpty(service.ServiceId))
        {
            return BadRequest("ServiceId is required");
        }

        await serviceRegistry.AddManualServiceAsync(service);
        return Ok();
    }

    [HttpDelete("manual/{serviceId}")]
    public async Task<ActionResult> RemoveManualService(string serviceId)
    {
        await serviceRegistry.RemoveManualServiceAsync(serviceId);
        return Ok();
    }
}