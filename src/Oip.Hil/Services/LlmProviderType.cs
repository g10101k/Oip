namespace Oip.Hil.Services;

/// <summary>
/// Kind of LLM API the provider speaks.
/// </summary>
public enum LlmProviderType
{
    /// <summary>
    /// OpenAI REST API (also any OpenAI-compatible endpoint such as Azure OpenAI, Ollama, vLLM, OpenRouter).
    /// </summary>
    OpenAi = 0
}
