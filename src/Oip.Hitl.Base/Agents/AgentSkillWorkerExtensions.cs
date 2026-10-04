using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Extensions;
using Oip.Base.Security.ServiceAccount;
using Oip.Hitl.Base.Settings;
using Oip.Hitl.Base.Workflows;

namespace Oip.Hitl.Base.Agents;

/// <summary>
/// Registration of a skill worker: a service that hosts agent tools on its own task queue.
/// </summary>
public static class AgentSkillWorkerExtensions
{
    /// <summary>
    /// Registers the Temporal worker of the tool activities on <see cref="TemporalSettings.TaskQueue"/>, the
    /// registration of their tools in Oip.Hitl and <see cref="IAgentUserTokenProvider"/>. Oip.Hitl is called with
    /// the service account token.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="temporalSettings">Temporal connection and the task queue of the worker.</param>
    /// <param name="hitlUrl">Address of Oip.Hitl.</param>
    /// <param name="serviceAccount">Service account Oip.Hitl is called with.</param>
    /// <param name="configure">Registers the activities with the tools; they must be registered in DI too.</param>
    public static IServiceCollection AddAgentSkillWorker(this IServiceCollection services,
        TemporalSettings temporalSettings, string hitlUrl, ServiceAccountOptions serviceAccount,
        Action<OipWorkflowOptions> configure)
    {
        services.AddOipTemporalWorker(temporalSettings, configure);
        var client = services.AddGrpcClient<GrpcAgentService.GrpcAgentServiceClient>(options =>
            options.Address = new Uri(hitlUrl));
        if (serviceAccount.AcceptAnyServerCertificate)
            client.ConfigurePrimaryHttpMessageHandler(OipModuleApplication.CreateDevelopmentHttpClientHandler);
        client.AddServiceAccountAuthorization(serviceAccount);
        services.AddSingleton<IAgentUserTokenProvider, GrpcAgentUserTokenProvider>();
        services.AddHostedService<AgentToolRegistrationService>();
        return services;
    }
}
