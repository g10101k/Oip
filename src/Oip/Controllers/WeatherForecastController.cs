using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Controllers.Api;
using Oip.Data.Dtos;
using Oip.Data.Repositories;

#pragma warning disable CS1591

namespace Oip.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly FeatureRepository _featureRepository;

    private readonly List<SecurityResponse> _defaultFeatureSecurity = new()
    {
        new() { Code = "read", Name = "Read", Description = "Can view this feature", Roles = ["admin"] },
        new() { Code = "edit", Name = "Edit", Description = "Can edit data", Roles = ["admin"] },
        new() { Code = "delete", Name = "Delete", Description = "Can delete edit data", Roles = ["admin"] },
    };

    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
#pragma warning disable S4487
    private readonly ILogger<WeatherForecastController> _logger;
#pragma warning restore S4487
    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="featureRepository"></param>
    public WeatherForecastController(ILogger<WeatherForecastController> logger, FeatureRepository featureRepository)
    {
        _logger = logger;
        _featureRepository = featureRepository;
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public IActionResult Get()
    {
        return Ok(Enumerable.Range(1, 5).Select(index => new WeatherForecastResponse
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray());
    }

    /// <summary>
    /// Get security for instance id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Roles = "admin")]
    [Route("get-security")]
    public async Task<List<SecurityResponse>> GetSecurity(int id)
    {
        var roleRightPair = await _featureRepository.GetSecurityByInstanceId(id);
        var result = new List<SecurityResponse>();
        foreach (var security in _defaultFeatureSecurity)
        {
            security.Roles = roleRightPair.Where(x => x.Right == security.Code).Select(x => x.Role).Distinct().ToList();
            result.Add(security);
        }

        return result;
    }

    /// <summary>
    /// Update security
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut]
    [Authorize(Roles = "admin")]
    [Route("put-security")]
    public async Task<IActionResult> PutSecurity(PutSecurityRequest request)
    {
        List<FeatureSecurityDto> featureSecurityDtos = new();
        foreach (var security in request.Securities)
        {
            if (security.Roles is null) continue;
            foreach (var role in security.Roles)
            {
                featureSecurityDtos.Add(new FeatureSecurityDto()
                {
                    Right = security.Code,
                    Role = role
                });
            }
        }

        await _featureRepository.UpdateInstanceSecurity(request.Id, featureSecurityDtos);
        return Ok();
    }
}