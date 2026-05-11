namespace Oip.Cli;

public sealed record TargetProject(
    string ProjectPath,
    string ProjectDirectory,
    string ProjectName,
    string RootNamespace,
    string SpaRoot,
    string AngularProjectPath);
