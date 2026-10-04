using Oip.Base.Settings.Attributes;

namespace Oip.Hitl.Base.Settings;

/// <summary>
/// Connection to the Temporal server and the task queue the application worker polls.
/// </summary>
public class TemporalSettings
{
    /// <summary>
    /// Temporal frontend address (host:port).
    /// </summary>
    public string Address { get; set; } = "localhost:7233";

    /// <summary>
    /// Temporal namespace.
    /// </summary>
    public string Namespace { get; set; } = "default";

    /// <summary>
    /// Task queue the workflows are started on and the worker polls.
    /// </summary>
    public string TaskQueue { get; set; } = "oip";

    /// <summary>
    /// API key sent as a bearer token with every call (Temporal Cloud). Enables TLS unless it is configured explicitly.
    /// </summary>
    [SecretSetting(Required = false)]
    public string? ApiKey { get; set; }

    /// <summary>
    /// TLS connection settings.
    /// </summary>
    public TemporalTlsSettings Tls { get; set; } = new();
}

/// <summary>
/// TLS connection to Temporal. TLS is used when <see cref="Enabled"/> is set, when any of the files is set,
/// or when <see cref="TemporalSettings.ApiKey"/> is set.
/// </summary>
public class TemporalTlsSettings
{
    /// <summary>
    /// Connects over TLS validated against the system root certificates.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Path to the PEM root CA certificate the server certificate is validated against, instead of the system ones.
    /// </summary>
    public string? CaPath { get; set; }

    /// <summary>
    /// Path to the PEM client certificate for mTLS; requires <see cref="KeyPath"/>.
    /// </summary>
    public string? CertPath { get; set; }

    /// <summary>
    /// Path to the PEM client private key for mTLS; requires <see cref="CertPath"/>.
    /// </summary>
    public string? KeyPath { get; set; }

    /// <summary>
    /// Server name used for SNI and certificate validation, when it differs from the host in the address.
    /// </summary>
    public string? ServerName { get; set; }
}
