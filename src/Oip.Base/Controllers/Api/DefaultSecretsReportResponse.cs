using Oip.Base.Security.DefaultSecrets;

namespace Oip.Base.Controllers.Api;

/// <summary>
/// Administration view of the secret settings that still hold a value shipped with the repository.
/// </summary>
public sealed class DefaultSecretsReportResponse
{
    /// <summary>
    /// Whether at least one secret setting needs attention.
    /// </summary>
    public bool HasFindings { get; set; }

    /// <summary>
    /// Number of secret bearing settings that were inspected.
    /// </summary>
    public int CheckedCount { get; set; }

    /// <summary>
    /// Whether the administration UI should show the banner. Findings are reported in every environment, but the
    /// banner is only raised in Production - a developer running the sample configuration already knows it holds
    /// sample secrets, and a permanent banner would train them to ignore it.
    /// </summary>
    public bool ShowBanner { get; set; }

    /// <summary>
    /// Settings that still hold a shipped default or are required but empty.
    /// </summary>
    public List<DefaultSecretFindingResponse> Findings { get; set; } = new();

    /// <summary>
    /// Builds the response from a validation result.
    /// </summary>
    /// <param name="report">The validation result.</param>
    /// <param name="isProduction">Whether the service runs in the Production environment.</param>
    /// <returns>The administration view of the report.</returns>
    public static DefaultSecretsReportResponse Create(DefaultSecretsReport report, bool isProduction) => new()
    {
        HasFindings = report.HasFindings,
        ShowBanner = report.HasFindings && isProduction,
        CheckedCount = report.CheckedCount,
        Findings = report.Findings.Select(finding => new DefaultSecretFindingResponse
        {
            ConfigKey = finding.ConfigKey,
            Kind = finding.Kind,
            EnvironmentVariable = finding.EnvironmentVariable
        }).ToList()
    };
}

/// <summary>
/// A single secret setting that needs attention.
/// </summary>
public sealed class DefaultSecretFindingResponse
{
    /// <summary>
    /// Configuration key of the setting, for example <c>SecurityService:ClientSecret</c>.
    /// </summary>
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>
    /// Kind of the problem.
    /// </summary>
    public DefaultSecretFindingKind Kind { get; set; }

    /// <summary>
    /// Environment variable that overrides the setting.
    /// </summary>
    public string EnvironmentVariable { get; set; } = string.Empty;
}
