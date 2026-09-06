using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Oip.Base.Settings;

namespace Oip.Base.Security.DefaultSecrets;

/// <summary>
/// Checks whether secret bearing settings still hold the values shipped with the repository.
/// </summary>
public class DefaultSecretsValidator(
    ISettings settings,
    IConfiguration configuration,
    IOptions<DefaultSecretsOptions> options)
{
    /// <summary>
    /// Inspects the settings object graph and the additionally registered configuration keys.
    /// </summary>
    /// <returns>The findings together with the number of inspected settings.</returns>
    public DefaultSecretsReport Validate()
    {
        var ignored = new HashSet<string>(options.Value.IgnoredSecretKeys, StringComparer.OrdinalIgnoreCase);

        var descriptors = SecretSettingsScanner.Scan(settings)
            .Concat(KnownDefaultSecrets.RawSecrets.Select(ToDescriptor))
            .Where(x => !ignored.Contains(x.ConfigKey))
            .ToList();

        var findings = descriptors.Select(Inspect).OfType<DefaultSecretFinding>().ToList();

        return new DefaultSecretsReport(findings, descriptors.Count);
    }

    private SecretSettingDescriptor ToDescriptor(RawSecretSetting raw) =>
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
