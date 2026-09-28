using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oip.Hil.Base.Settings;
using Temporalio.Client;
using Temporalio.Workflows;

namespace Oip.Hil.Base.Workflows;

/// <summary>
/// Workflows and activities hosted by the application worker.
/// </summary>
public class OipWorkflowOptions
{
    private readonly List<Type> workflows = [];
    private readonly List<Type> activities = [];

    /// <summary>
    /// Registered workflow types.
    /// </summary>
    public IReadOnlyList<Type> Workflows => workflows;

    /// <summary>
    /// Registered activity types; an instance is resolved from a new DI scope for each activity call.
    /// </summary>
    public IReadOnlyList<Type> Activities => activities;

    /// <summary>
    /// Temporal names of the registered workflows that derive from <see cref="UserWorkflowBase"/>.
    /// </summary>
    public IReadOnlySet<string> UserWorkflowNames => workflows
        .Where(type => type.IsAssignableTo(typeof(UserWorkflowBase)))
        .Select(type => WorkflowDefinition.Create(type).Name!)
        .ToHashSet();

    /// <summary>
    /// Registers a workflow in the worker.
    /// </summary>
    public OipWorkflowOptions AddWorkflow<TWorkflow>()
    {
        workflows.Add(typeof(TWorkflow));
        return this;
    }

    /// <summary>
    /// Registers the <see cref="Temporalio.Activities.ActivityAttribute"/> methods of the type in the worker. The type
    /// must be registered in DI; it is resolved from a new scope for each activity call.
    /// </summary>
    public OipWorkflowOptions AddActivities<TActivities>()
    {
        activities.Add(typeof(TActivities));
        return this;
    }
}

/// <summary>
/// Registration of the Temporal client, worker and user step services.
/// </summary>
public static class WorkflowServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Temporal client, the worker for the configured workflows, <see cref="UserStepService"/> and
    /// <see cref="WorkflowFileStorage"/>.
    /// </summary>
    public static IServiceCollection AddOipWorkflows(this IServiceCollection services, TemporalSettings settings,
        WorkflowFileStorageSettings fileStorageSettings, Action<OipWorkflowOptions> configure)
    {
        var options = new OipWorkflowOptions();
        configure(options);

        services.AddSingleton(settings);
        services.AddSingleton(options);
        // Built right away, so a wrong TLS configuration stops the startup instead of the first call.
        var connectOptions = CreateConnectOptions(settings);
        services.AddSingleton<ITemporalClient>(provider =>
        {
            connectOptions.LoggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return TemporalClient.CreateLazy(connectOptions);
        });
        services.AddHostedService<TemporalWorkerService>();
        services.AddSingleton(fileStorageSettings);
        services.AddSingleton<WorkflowFileStorage>();
        services.AddScoped<UserStepService>();
        return services;
    }

    private static TemporalClientConnectOptions CreateConnectOptions(TemporalSettings settings)
    {
        return new TemporalClientConnectOptions(settings.Address)
        {
            Namespace = settings.Namespace,
            ApiKey = string.IsNullOrWhiteSpace(settings.ApiKey) ? null : settings.ApiKey,
            Tls = CreateTlsOptions(settings.Tls)
        };
    }

    private static TlsOptions? CreateTlsOptions(TemporalTlsSettings tls)
    {
        if (string.IsNullOrWhiteSpace(tls.CertPath) != string.IsNullOrWhiteSpace(tls.KeyPath))
            throw new InvalidOperationException(
                "Temporal:Tls:CertPath and Temporal:Tls:KeyPath must be set together for mTLS");

        var configured = tls.Enabled || !string.IsNullOrWhiteSpace(tls.CaPath) ||
                         !string.IsNullOrWhiteSpace(tls.CertPath) || !string.IsNullOrWhiteSpace(tls.ServerName);
        if (!configured) return null;

        return new TlsOptions
        {
            ServerRootCACert = ReadPem(tls.CaPath, "Temporal:Tls:CaPath"),
            ClientCert = ReadPem(tls.CertPath, "Temporal:Tls:CertPath"),
            ClientPrivateKey = ReadPem(tls.KeyPath, "Temporal:Tls:KeyPath"),
            Domain = string.IsNullOrWhiteSpace(tls.ServerName) ? null : tls.ServerName
        };
    }

    private static byte[]? ReadPem(string? path, string key)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        if (!File.Exists(path))
            throw new InvalidOperationException($"{key}: file '{path}' not found");
        return File.ReadAllBytes(path);
    }
}
