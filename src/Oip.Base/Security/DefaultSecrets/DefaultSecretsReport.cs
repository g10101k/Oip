namespace Oip.Base.Security.DefaultSecrets;

/// <summary>
/// Result of a default secret validation pass.
/// </summary>
/// <param name="Findings">Settings that still hold a shipped default or are required but empty.</param>
/// <param name="CheckedCount">Number of secret bearing settings that were inspected.</param>
public sealed record DefaultSecretsReport(IReadOnlyList<DefaultSecretFinding> Findings, int CheckedCount)
{
    /// <summary>
    /// Whether at least one problem was detected.
    /// </summary>
    public bool HasFindings => Findings.Count > 0;

    /// <summary>
    /// An empty report.
    /// </summary>
    public static DefaultSecretsReport Empty { get; } = new([], 0);
}
