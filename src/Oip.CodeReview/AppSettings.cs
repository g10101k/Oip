namespace Oip.CodeReview;

/// <summary>
/// Represents the application settings.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Working directory for the code review process.
    /// </summary>
    public string WorkDir { get; set; } = null!;

    /// <summary>
    /// The source branch for the code review process.
    /// </summary>
    public string SourceBranch { get; set; } = null!;

    /// <summary>
    /// The target branch for code review.
    /// </summary>
    public string TargetBranch { get; set; } = "main";

    /// <summary>
    /// File for diff
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// When true, only prompts the user for confirmation before executing actions.
    /// </summary>
    public bool PromptOnly { get; set; }

    /// <summary>
    /// Represents the Ollama settings.
    /// </summary>
    public OllamaSettings Ollama { get; set; } = null!;
}

/// <summary>
/// Represents the Ollama settings.
/// </summary>
public class OllamaSettings
{
    /// <summary>
    /// The URL of the Ollama server.
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// The name of the Ollama model to use for code review.
    /// </summary>
    public string Model { get; set; } = null!;
}