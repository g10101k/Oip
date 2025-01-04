namespace Oip.Controllers.Api;

/// <summary>
/// Response
/// </summary>
public class WeatherForecastResponse
{
    /// <summary>
    /// Date
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Temp in ºC
    /// </summary>
    public int TemperatureC { get; set; }

    /// <summary>
    /// Temp in ºF
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    /// <summary>
    /// Summary
    /// </summary>
    public string? Summary { get; set; }
}