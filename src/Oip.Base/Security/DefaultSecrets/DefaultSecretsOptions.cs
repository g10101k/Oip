namespace Oip.Base.Security.DefaultSecrets;

/// <summary>
/// Options controlling the default secret validation, bound from the <c>Security</c> configuration section.
/// </summary>
public class DefaultSecretsOptions
{
    /// <summary>
    /// Name of the configuration section the options are bound from.
    /// </summary>
    public const string SectionName = "Security";

    /// <summary>
    /// Whether the validation runs at all. Default value = true.
    /// </summary>
    public bool ValidateDefaultSecrets { get; set; } = true;

    /// <summary>
    /// Whether the application refuses to start when a shipped default secret is still in use.
    /// Ignored outside the Production environment. Default value = false.
    /// </summary>
    public bool FailOnDefaultSecrets { get; set; }

    /// <summary>
    /// Configuration keys excluded from the validation, for example a secret that is intentionally unused.
    /// </summary>
    public string[] IgnoredSecretKeys { get; set; } = [];
}
