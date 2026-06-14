namespace Oip.Base.Settings;

/// <summary>
/// CORS settings
/// </summary>
public class CorsItemSettings
{
    /// <summary>
    /// CORS policy name
    /// </summary>
    public string PolicyName { get; set; } = string.Empty;

    /// <summary>
    /// Allowed CORS origins
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];

    /// <summary>
    /// Allows any CORS request header
    /// </summary>
    public bool AllowAnyHeader { get; set; } = true;

    /// <summary>
    /// Allows any CORS request method
    /// </summary>
    public bool AllowAnyMethod { get; set; } = true;

    /// <summary>
    /// Allows credentials in CORS requests
    /// </summary>
    public bool AllowCredentials { get; set; } = true;
}