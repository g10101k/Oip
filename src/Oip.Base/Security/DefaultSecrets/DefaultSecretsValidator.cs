using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Oip.Base.Settings;

namespace Oip.Base.Security.DefaultSecrets;

/// <summary>
/// Checks whether secret bearing settings still hold the values shipped with the repository. The result is computed
/// once and cached, so the startup log, the health check and the administration UI report the same state.
/// </summary>
public class DefaultSecretsValidator(
    ISettings settings,
    IConfiguration configuration,
    IOptions<DefaultSecretsOptions> options)
{
    private readonly Lazy<DefaultSecretsReport> _report = new(
        () => Build(settings, configuration, options.Value), LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// The cached validation result. Configuration is immutable after startup, so the check runs only once.
    /// </summary>
    public DefaultSecretsReport Report => options.Value.ValidateDefaultSecrets
        ? _report.Value
        : DefaultSecretsReport.Empty;

    /// <summary>
    /// Inspects the settings object graph and the additionally registered configuration keys, bypassing the cache.
    /// </summary>
    /// <returns>The findings together with the number of inspected settings.</returns>
    public DefaultSecretsReport Validate() => Build(settings, configuration, options.Value);

    private static DefaultSecretsReport Build(ISettings settings, IConfiguration configuration,
        DefaultSecretsOptions options)
    {
        var ignored = new HashSet<string>(options.IgnoredSecretKeys, StringComparer.OrdinalIgnoreCase);

        var descriptors = SecretSettingsScanner.Scan(settings)
            .Concat(KnownDefaultSecrets.RawSecrets.Select(x => ToDescriptor(x, configuration)))
            .Where(x => !ignored.Contains(x.ConfigKey))
            .ToList();

        var findings = descriptors.Select(Inspect).OfType<DefaultSecretFinding>().ToList();

        return new DefaultSecretsReport(findings, descriptors.Count);
    }

    private static SecretSettingDescriptor ToDescriptor(RawSecretSetting raw, IConfiguration configuration) =>
        new(raw.ConfigKey, configuration[raw.ConfigKey], raw.InsecureValues, raw.Required, raw.OverrideHint);

    private static DefaultSecretFinding? Inspect(SecretSettingDescriptor descriptor)
    {
        if (string.IsNullOrWhiteSpace(descriptor.Value))
        {
            return descriptor.Required
                ? new DefaultSecretFinding(descriptor.ConfigKey, DefaultSecretFindingKind.Missing,
                    descriptor.OverrideHint)
                : null;
        }

        var isDefault = descriptor.InsecureValues
            .Any(x => string.Equals(x, descriptor.Value, StringComparison.Ordinal));

        return isDefault
            ? new DefaultSecretFinding(descriptor.ConfigKey, DefaultSecretFindingKind.ShippedDefault,
                descriptor.OverrideHint)
            : null;
    }
}
