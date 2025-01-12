using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Controllers.Api;
using Oip.Data.Repositories;

namespace Oip.Controllers;

/// <summary>
/// Feature controller example
/// </summary>
[ApiController]
[Route("api/weather")]
public class WeatherForecastController : BaseFeatureController<WeatherFeatureSettings>
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

    /// <summary>
    /// Get example data
    /// </summary>
    /// <param name="dayCount"></param>
    /// <returns></returns>
    [HttpGet("get")]
    [Authorize()]
    public IActionResult Get(int dayCount)
    {
        return Ok(Enumerable.Range(1, dayCount).Select(index => new WeatherForecastResponse
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray());
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
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

/// <summary>
/// Feature settings
/// </summary>
public class WeatherFeatureSettings
{
    /// <summary>
    /// Day count
    /// </summary>
    public int DayCount { get; set; } = 5;
}