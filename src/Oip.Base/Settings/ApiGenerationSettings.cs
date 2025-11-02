namespace Oip.Base.Settings;

/// <summary>
/// Represents settings for API generation.
/// </summary>
public class ApiGenerationSettings
{
    /// <summary>
    /// The name of the Swagger document to generate a client for.
    /// </summary>
    public string DocumentName { get; set; } = null!;

    /// <summary>
    /// The path to output the generated API client.
    /// </summary>
    public string OutputPath { get; set; } = null!;
}