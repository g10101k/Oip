namespace Oip.CodeReview;

public class AppSettings
{
    public string WorkDir { get; set; }
    public string SourceBranch { get; set; }
    public OllamaSettings Ollama { get; set; } 
    public string TargetBranch { get; set; }
}

public class OllamaSettings
{
    public string Url { get; set; }
    public string Model { get; set; }
}