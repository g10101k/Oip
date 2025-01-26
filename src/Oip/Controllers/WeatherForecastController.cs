﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Controllers.Api;
using Oip.Data.Repositories;

namespace Oip.Controllers;

/// <summary>
/// Module controller example
/// </summary>
[ApiController]
[Route("api/weather")]
public class WeatherForecastController : BaseModuleController<WeatherModuleSettings>
{
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="moduleRepository"></param>
    public WeatherForecastController(ModuleRepository moduleRepository) : base(moduleRepository)
    {
    }

    /// <summary>
    /// Get example data
    /// </summary>
    /// <param name="dayCount"></param>
    /// <returns></returns>
    [HttpGet("get")]
    [Authorize()]
    [ProducesResponseType<List<WeatherForecastResponse>>(StatusCodes.Status200OK)]

    public IActionResult Get(int dayCount)
    {
        return Ok(Enumerable.Range(1, dayCount).Select(index => new WeatherForecastResponse
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToList());
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public override List<SecurityResponse> GetModuleRights()
    {
        return new()
        {
            new() { Code = "read", Name = "Read", Description = "Can view this module", Roles = ["admin"] },
            new() { Code = "edit", Name = "Edit", Description = "Can edit data", Roles = ["admin"] },
            new() { Code = "delete", Name = "Delete", Description = "Can delete edit data", Roles = ["admin"] },
        };
    }
}

/// <summary>
/// Module settings
/// </summary>
public class WeatherModuleSettings
{
    /// <summary>
    /// Day count
    /// </summary>
    public int DayCount { get; set; } = 5;
}