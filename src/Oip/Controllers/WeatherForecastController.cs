using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Controllers.Api;
using Oip.Data.Repositories;

#pragma warning disable CS1591

namespace Oip.Controllers;

[ApiController]
[Route("api/weather")]
public class WeatherForecastController : BaseFeatureController
{
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="featureRepository"></param>
    public WeatherForecastController(FeatureRepository featureRepository) : base(featureRepository)
    {
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

    public override List<SecurityResponse> GetFeatureRights()
    {
        return new()
        {
            new() { Code = "read", Name = "Read", Description = "Can view this feature", Roles = ["admin"] },
            new() { Code = "edit", Name = "Edit", Description = "Can edit data", Roles = ["admin"] },
            new() { Code = "delete", Name = "Delete", Description = "Can delete edit data", Roles = ["admin"] },
        };
    }
}