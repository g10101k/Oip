using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///  ConfigurationBuilder extension
/// </summary>
public static class ConfigurationBuilderExtension
{
    /// <summary>
    /// Add SPA proxy config
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IConfigurationBuilder AddSpaConfig(this IConfigurationBuilder builder)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "spa.proxy.json");
        
        return File.Exists(path) ? builder.AddJsonFile(path) : builder;
    }
}