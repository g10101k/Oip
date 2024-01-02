namespace Oip.Base.Settings;

/// <summary>
/// Spa proxy server config
/// </summary>
public class SpaDevelopmentServerSettings
{
    /// <summary>
    /// Server url
    /// </summary>
    public string ServerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Launch command
    /// </summary>
    public string LaunchCommand { get; set; } = string.Empty;
    
    /// <summary>
    /// Timeout 
    /// </summary>
    public int MaxTimeoutInSeconds { get; set; }

    /// <summary>
    /// TimeSpan
    /// </summary>
    public TimeSpan MaxTimeout => TimeSpan.FromSeconds(MaxTimeoutInSeconds);

    /// <summary>
    /// Working Directory
    /// </summary>
    public string WorkingDirectory { get; set; } = string.Empty;
}