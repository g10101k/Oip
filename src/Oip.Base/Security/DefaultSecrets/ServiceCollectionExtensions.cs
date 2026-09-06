using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Runtime;

namespace Oip.Base.Security.DefaultSecrets;

/// <summary>
/// Registration of the default secret validation.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the startup task that reports secret settings still holding a value shipped with the repository.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddDefaultSecretsValidation(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DefaultSecretsOptions>(configuration.GetSection(DefaultSecretsOptions.SectionName));
        services.AddScoped<DefaultSecretsValidator>();
        services.AddStartupTask<DefaultSecretsStartupTask>();

        return services;
    }
}
