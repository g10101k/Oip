namespace Oip.Base.Security.DefaultSecrets;

/// <summary>
/// A secret bearing setting discovered on a settings object graph.
/// </summary>
/// <param name="ConfigKey">Configuration key of the setting.</param>
/// <param name="Value">Effective value of the setting.</param>
/// <param name="InsecureValues">Values shipped with the repository.</param>
/// <param name="Required">Whether an empty value has to be reported as a finding.</param>
/// <param name="OverrideHint">Hint explaining how to supply a real value.</param>
public sealed record SecretSettingDescriptor(
    string ConfigKey,
    string? Value,
    IReadOnlyList<string> InsecureValues,
    bool Required,
    string? OverrideHint);
