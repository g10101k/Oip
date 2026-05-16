namespace Oip.Cli;

public sealed record ModuleName(
    string BaseName,
    string ClassPrefix,
    string ControllerClassName,
    string SettingsClassName,
    string ComponentClassName,
    string KebabName)
{
    public string RoutePath => $"{KebabName}-module";
    public string ControllerRoute => $"api/{RoutePath}";
    public string ComponentFolder => RoutePath;
    public string ComponentFileName => $"{RoutePath}.component.ts";
}
