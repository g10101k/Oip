using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oip.Base.Security.ServiceAccount;
using Oip.Base.Settings;

namespace Oip.Base.Extensions;

/// <summary>
/// Extension methods that authorize calls between OIP services with the service account token.
/// </summary>
public static class ServiceAccountExtensions
{
    /// <summary>
    /// Registers <see cref="IServiceAccountTokenProvider" /> for the specified service account.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">Service account connection parameters.</param>
    public static IServiceCollection AddServiceAccountTokenProvider(this IServiceCollection services,
        ServiceAccountOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (services.Any(x => x.ServiceType == typeof(IServiceAccountTokenProvider)))
            return services;

        services.AddHttpClient(ServiceAccountTokenProvider.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => options.AcceptAnyServerCertificate
                ? OipModuleApplication.CreateDevelopmentHttpClientHandler()
                : new HttpClientHandler());
        services.TryAddSingleton<IServiceAccountTokenProvider>(sp => new ServiceAccountTokenProvider(
            sp.GetRequiredService<IHttpClientFactory>(),
            options,
            sp.GetService<TimeProvider>() ?? TimeProvider.System));

        return services;
    }

    /// <summary>
    /// Adds the service account access token to every request of the client, e.g. a gRPC client registered
    /// with <c>AddGrpcClient</c>.
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <param name="options">Service account connection parameters.</param>
    public static IHttpClientBuilder AddServiceAccountAuthorization(this IHttpClientBuilder builder,
        ServiceAccountOptions options)
    {
        builder.Services.AddServiceAccountTokenProvider(options);
        builder.AddHttpMessageHandler(sp =>
            new ServiceAccountAuthorizationHandler(sp.GetRequiredService<IServiceAccountTokenProvider>()));
        return builder;
    }

    /// <summary>
    /// Adds the token of the Keycloak client configured in <see cref="ISettings.SecurityService" /> to every
    /// request of the client, e.g. a gRPC client registered with <c>AddGrpcClient</c>.
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <param name="settings">Application settings.</param>
    public static IHttpClientBuilder AddServiceAccountAuthorization(this IHttpClientBuilder builder,
        ISettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return builder.AddServiceAccountAuthorization(
            ServiceAccountOptions.FromSecurityService(settings.SecurityService, settings.IsDevelopment()));
    }
}
