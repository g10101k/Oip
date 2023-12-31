using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Oip.Settings.Providers;

/// <summary>
/// EFCore configuration source
/// </summary>
/// <typeparam name="TAppSettings"></typeparam>
public class EfConfigurationSource<TAppSettings> : IConfigurationSource where TAppSettings : class, IAppSettings
{
    private readonly TAppSettings _appSettings;
    private readonly AppSettingsOptions _optionsAction;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="optionsAction"></param>
    /// <param name="appSettings"></param>
    public EfConfigurationSource(AppSettingsOptions optionsAction, TAppSettings appSettings)
    {
        _optionsAction = optionsAction;
        _appSettings = appSettings;
    }

    /// <summary>
    /// Build application settings provider
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new EfConfigurationProvider<TAppSettings>(_optionsAction, _appSettings);
    }
}