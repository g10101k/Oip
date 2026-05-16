namespace Oip.Cli;

public sealed record ModuleGenerationResult(IReadOnlyList<string> CreatedFiles, IReadOnlyList<string> ChangedFiles);
