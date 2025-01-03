using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Module.Example.Controllers;

#pragma warning disable CS1591

namespace Oip.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
#pragma warning disable S4487
    private readonly ILogger<WeatherForecastController> _logger;
#pragma warning restore S4487

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public IActionResult Get()
    {
        return Ok(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray());
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    [Route("get-security")]
    public IActionResult GetSecurity(string id)
    {
        return Ok(new WeatherForecastSecurityDto()
        {
            Read = ["admin"]
        });
    }

    [HttpPut]
    [Authorize(Roles = "admin")]
    [Route("get-security")]
    public IActionResult PutSecurity(string id, WeatherForecastSecurityDto security)
    {
        
        return Ok(security);
    }
}
#pragma warning restore CS1591


public class WeatherForecastSecurityDto
{
    public List<string> Read { get; set; }
}