namespace Oip.Base.Security.DefaultSecrets;

/// <summary>
/// Kind of problem detected for a secret bearing setting.
/// </summary>
public enum DefaultSecretFindingKind
{
    /// <summary>
    /// The effective value is still one of the values shipped with the repository.
    /// </summary>
    ShippedDefault = 0,

    /// <summary>
    /// The setting is required but the effective value is empty.
    /// </summary>
    Missing = 1
}

/// <summary>
/// A single problem detected for a secret bearing setting.
/// </summary>
/// <param name="ConfigKey">Configuration key of the setting, for example <c>SecurityService:ClientSecret</c>.</param>
/// <param name="Kind">Kind of the problem.</param>
/// <param name="OverrideHint">Hint explaining how to supply a real value.</param>
public sealed record DefaultSecretFinding(string ConfigKey, DefaultSecretFindingKind Kind, string? OverrideHint)
{
    /// <summary>
    /// Environment variable that overrides the setting.
    /// </summary>
    public string EnvironmentVariable => ConfigKey.Replace(":", "__", StringComparison.Ordinal);

    /// <summary>
    /// Builds an operator facing description of the finding.
    /// </summary>
    public string Describe(string applicationName)
    {
        var problem = Kind == DefaultSecretFindingKind.ShippedDefault
            ? "still holds the default value shipped with the repository"
            : "is required but empty";

        var hint = OverrideHint ??
                   $"Override it with the environment variable '{EnvironmentVariable}', with user-secrets, " +
                   "or with your secret store before using this installation.";

        return $"[{applicationName}] Configuration key '{ConfigKey}' {problem}. {hint}";
    }
}
