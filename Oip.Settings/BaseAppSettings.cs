﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Oip.Settings.Attributes;
using Oip.Settings.Enums;
using Oip.Settings.Helpers;
using Oip.Settings.Providers;

namespace Oip.Settings;

/// <summary>
/// Base application settings class
/// </summary>
/// <typeparam name="TAppSettings"></typeparam>
public class BaseAppSettings<TAppSettings> : IAppSettings where TAppSettings : class, IAppSettings
{
    private static TAppSettings? _instance;

    // ReSharper disable once StaticMemberInGenericType
#pragma warning disable S2743
    private static AppSettingsOptions? _appSettingsOptions;
#pragma warning restore S2743

    /// <summary>
    /// Singleton application settings
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static TAppSettings Instance
    {
        get
        {
            if (_instance != null)
                return _instance;
            _instance = (TAppSettings)(Activator.CreateInstance(typeof(TAppSettings)) ??
                                       throw new InvalidOperationException());
            BindTemporaryConfiguration(_instance);
            BindMainConfiguration(_instance);
            return _instance;
        }
    }

    /// <inheritdoc />
    [NotSaveToDb]
    public string ConnectionString { get; set; } = default!;

    /// <inheritdoc />
    [NotSaveToDb]
    public string NormalizedConnectionString { get; set; } = default!;

    /// <inheritdoc />
    [NotSaveToDb]
    public XpoProvider Provider { get; set; } = XpoProvider.InMemoryDataStore;

    /// <inheritdoc />
    [NotSaveToDb]
    public AppSettingsOptions AppSettingsOptions => _appSettingsOptions!;

    /// <summary>
    /// Initialize app settings
    /// </summary>
    /// <param name="appSettingsOptions"></param>
    /// <returns></returns>
    public static TAppSettings Initialize(AppSettingsOptions appSettingsOptions)
    {
        _appSettingsOptions = appSettingsOptions;
        return Instance;
    }

#pragma warning disable S107
    /// <summary>
    /// Initialize app settings
    /// </summary>
    /// <param name="programArguments"></param>
    /// <param name="useEfCoreProvider"></param>
    /// <param name="jsonFileName"></param>
    /// <param name="jsonFileNameDevelopment"></param>
    /// <param name="appSettingsTable"></param>
    /// <param name="appSettingsSchema"></param>
    /// <param name="builder"></param>
    /// <param name="normalizeConnectionString"></param>
    /// <returns></returns>
    public static TAppSettings Initialize(string[]? programArguments = null, bool? useEfCoreProvider = null,
        bool? normalizeConnectionString = null, string? jsonFileName = null, string? jsonFileNameDevelopment = null,
        string? appSettingsTable = null, string? appSettingsSchema = null,
        Action<DbContextOptionsBuilder, XpoProvider, string>? builder = null)
    {
        _appSettingsOptions = new AppSettingsOptions();
        if (jsonFileName is not null)
            _appSettingsOptions.JsonFileName = jsonFileName;
        if (jsonFileNameDevelopment is not null)
            _appSettingsOptions.JsonFileNameDevelopment = jsonFileNameDevelopment;
        if (programArguments is not null)
            _appSettingsOptions.ProgrammeArguments = programArguments;
        if (useEfCoreProvider is not null)
            _appSettingsOptions.UseEfCoreProvider = (bool)useEfCoreProvider;
        if (appSettingsTable is not null)
            _appSettingsOptions.AppSettingsTable = appSettingsTable;
        if (appSettingsSchema is not null)
            _appSettingsOptions.AppSettingsSchema = appSettingsSchema;
        if (builder is not null)
            _appSettingsOptions.Builder = builder;
        if (normalizeConnectionString is not null)
            _appSettingsOptions.NormalizeConnectionString = (bool)normalizeConnectionString;
        return Instance;
    }
#pragma warning restore S107

    private static void BindTemporaryConfiguration(TAppSettings instance)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Instance.AppSettingsOptions.JsonFileName, true, true)
            .AddJsonFile(Instance.AppSettingsOptions.JsonFileNameDevelopment, true, true)
            .AddEnvironmentVariables()
            .AddCommandLine(Instance.AppSettingsOptions.ProgrammeArguments)
            .Build();
        BindConfig(configuration, instance);
    }

    private static void BindMainConfiguration(TAppSettings instance)
    {
        if (Instance.AppSettingsOptions is null)
            throw new InvalidOperationException(
                $"{nameof(_appSettingsOptions)} is null, call {nameof(Initialize)} before use {nameof(Instance)}");

        var efConfigurationSource = new EfConfigurationSource<TAppSettings>(Instance.AppSettingsOptions, instance);

        var configurationBuilder = new ConfigurationBuilder();
        if (instance.AppSettingsOptions.UseEfCoreProvider)
            configurationBuilder.Add(efConfigurationSource);
        var configuration = configurationBuilder
            .AddJsonFile(Instance.AppSettingsOptions.JsonFileName, true, true)
            .AddJsonFile(Instance.AppSettingsOptions.JsonFileNameDevelopment, true, true)
            .AddEnvironmentVariables()
            .AddCommandLine(Instance.AppSettingsOptions.ProgrammeArguments)
            .Build();

        BindConfig(configuration, instance);
        ChangeToken.OnChange(() => configuration.GetReloadToken(), () => { BindConfig(configuration, instance); });
    }

    private static void BindConfig(IConfiguration config, TAppSettings instance)
    {
        config.Bind(instance);
        NormalizeConnectionString(instance);
    }

    private static void NormalizeConnectionString(TAppSettings instance)
    {
        if (!instance.AppSettingsOptions.NormalizeConnectionString)
            return;
        var connectionModel = ConnectionStringHelper.NormalizeConnectionString(instance.ConnectionString);
        instance.NormalizedConnectionString = connectionModel.NormalizeConnectionString;
        instance.Provider = connectionModel.Provider;
    }
}