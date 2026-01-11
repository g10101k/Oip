using Microsoft.EntityFrameworkCore;
using Oip.Base.Clients;
using Oip.Base.Extensions;
using Oip.Base.Settings;
using Oip.Notifications.Data.Contexts;

namespace Oip.Notifications.Extensions;

/// <summary>
/// Provides extension methods for configuring and migrating the user database context.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Migrates the user database to the latest version using the configured database context.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance to extend.</param>
    /// <exception cref="InvalidOperationException">Thrown when the <see cref="NotificationsDbContext"/> cannot be resolved from the service provider.</exception>
    public static void MigrateNotificationDatabase(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = serviceScope.ServiceProvider.GetService<NotificationsDbContext>()
                      ?? throw new InvalidOperationException();
        context.Database.Migrate();
    }


    /// <summary>
    /// Configures and registers the Keycloak client service with the specified settings.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> instance to extend.</param>
    /// <param name="settings">The base OIP module application settings containing security service configuration.</param>
    /// <remarks>
    /// This method sets up an HTTP client for Keycloak with retry policies and configures SSL certificate validation for development environments.
    /// </remarks>
    public static void AddKeycloakClients(this WebApplicationBuilder builder, IBaseOipModuleAppSettings settings)
    {
        var httpClientBuilder = builder.Services.AddHttpClient<KeycloakClient>(httpClient =>
        {
            var url = settings.SecurityService.DockerUrl ?? settings.SecurityService.BaseUrl;
            httpClient.BaseAddress = new Uri(url);
        }).AddPolicyHandler(OipModuleApplication.GetRetryPolicy());

        if (builder.Environment.IsDevelopment() && settings.SecurityService.DockerUrl is not null)
        {
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
            {
                HttpClientHandler handler = new HttpClientHandler();

                handler.ServerCertificateCustomValidationCallback =
                    (message, cert, chain, errors) => true;
                return handler;
            });
        }
    }
}