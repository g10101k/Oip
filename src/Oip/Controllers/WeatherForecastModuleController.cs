﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Constants;
using Oip.Base.Controllers;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Repositories;
using Oip.Controllers.Api;
using Oip.Properties;

namespace Oip.Controllers;

/// <summary>
/// Module controller example
/// </summary>
[ApiController]
[Route("api/weather-forecast-module")]
public class WeatherForecastModuleController : BaseModuleController<WeatherModuleSettings>
{
    private readonly string[] _summaries =
    [
        Resources.WeatherForecastController_Summaries_Freezing,
        Resources.WeatherForecastController_Summaries_Bracing,
        Resources.WeatherForecastController_Summaries_Chilly,
        Resources.WeatherForecastController_Summaries_Cool,
        Resources.WeatherForecastController_Summaries_Mild,
        Resources.WeatherForecastController_Summaries_Warm,
        Resources.WeatherForecastController_Summaries_Balmy,
        Resources.WeatherForecastController_Summaries_Hot,
        Resources.WeatherForecastController_Summaries_Sweltering,
        Resources.WeatherForecastController_Summaries_Scorching
    ];

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="moduleRepository"></param>
    public WeatherForecastModuleController(ModuleRepository moduleRepository) : base(moduleRepository)
    {
    }

    /// <summary>
    /// Get example data
    /// </summary>
    /// <param name="dayCount"></param>
    /// <returns></returns>
    [HttpGet("get-weather-forecast")]
    [Authorize]
    [ProducesResponseType<List<WeatherForecastResponse>>(StatusCodes.Status200OK)]
    public IActionResult Get(int dayCount)
    {
        return Ok(Enumerable.Range(1, dayCount).Select(index => new WeatherForecastResponse
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = _summaries[Random.Shared.Next(_summaries.Length)]
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
            new()
            {
                Code = SecurityConstants.Read,
                Name = Resources.WeatherForecastController_GetModuleRights_Read,
                Description = Resources.WeatherForecastController_GetModuleRights_Can_view_this_module,
                Roles = [SecurityConstants.AdminRole]
            },
            new()
            {
                Code = SecurityConstants.Edit, 
                Name = Resources.WeatherForecastController_GetModuleRights_Edit,
                Description = Resources.WeatherForecastController_GetModuleRights_Can_edit_data, 
                Roles = [SecurityConstants.AdminRole]
            },
            new()
            {
                Code = SecurityConstants.Delete,
                Name = Resources.WeatherForecastController_GetModuleRights_Delete,
                Description = Resources.WeatherForecastController_GetModuleRights_Can_delete_edit_data,
                Roles = [SecurityConstants.AdminRole]
            },
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