namespace Oip.Base.Settings.Attributes;

/// <summary>
/// Marks a settings property as secret bearing so that it can be discovered by the default secret validator.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SecretSettingAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretSettingAttribute"/> class.
    /// </summary>
    /// <param name="insecureValues">
    /// Values shipped with the repository that must never survive into a real installation.
    /// </param>
    public SecretSettingAttribute(params string[] insecureValues)
    {
        InsecureValues = insecureValues;
    }

    /// <summary>
    /// Values shipped with the repository that must never survive into a real installation.
    /// </summary>
    public IReadOnlyList<string> InsecureValues { get; }

    /// <summary>
    /// Configuration key of the setting. When not set, the key is derived from the property path.
    /// </summary>
    public string? ConfigKey { get; set; }

    /// <summary>
    /// Whether an empty value has to be reported as a finding. Default value = true.
    /// </summary>
    public bool Required { get; set; } = true;

    /// <summary>
    /// Additional hint shown to the operator explaining how to supply a real value.
    /// </summary>
    public string? OverrideHint { get; set; }
}
