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
    /// Contains settings related to the OpenAI API.
    /// </summary>
    public OpenAiApiSettings? OpenAiApiSettings { get; set; }

    /// <summary>
    /// List of folders to exclude from the code review process.
    /// </summary>
    public List<string> ExcludePatterns { get; set; } = [];

    /// <summary>
    /// Only new code changes during the code review process.
    /// </summary>
    public bool NewCodeOnly { get; set; }

    /// <summary>
    /// The file path to the system prompt file.
    /// </summary>
    public string SystemPromptFilePath { get; set; } = "prompt.txt";
}

/// <summary>
/// Represents the Open AI API settings.
/// </summary>
public class OpenAiApiSettings
{
    /// <summary>
    /// The ID of the model to use for the code review process.
    /// </summary>
    public string ModelId { get; set; } = "gemma3:27b";

    /// <summary>
    /// Base URL for the OpenAI API service.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// The API key used for authentication. Use secrets.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether to use streaming for the Open AI API calls.
    /// </summary>
    public bool UseStream { get; set; }
}