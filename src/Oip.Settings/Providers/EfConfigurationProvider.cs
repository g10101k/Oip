using System.Collections;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Oip.Settings.Attributes;
using Oip.Settings.Contexts;
using Oip.Settings.Entities;
using Oip.Settings.Enums;

namespace Oip.Settings.Providers;

/// <summary>
/// EF Core settings provider
/// </summary>
/// <typeparam name="TAppSettings"></typeparam>
public class EfConfigurationProvider<TAppSettings> : ConfigurationProvider where TAppSettings : class, IAppSettings
{
    private readonly AppSettingsOptions _appSettingsOptions;
    private readonly TAppSettings _settings;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="appSettingsOptions"></param>
    /// <param name="appSettings"></param>
    public EfConfigurationProvider(AppSettingsOptions appSettingsOptions, TAppSettings appSettings)
    {
        _appSettingsOptions = appSettingsOptions;
        _settings = appSettings;
    }

    /// <summary>
    /// Load settings
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public override void Load()
    {
        var builder = new DbContextOptionsBuilder<AppSettingsContext>();

        _appSettingsOptions.Builder(builder, _settings.Provider, _settings.ConnectionString);

        switch (_settings.Provider)
        {
            case XpoProvider.Postgres:
                using (var context = new PostgresMigrationContext(builder.Options, _appSettingsOptions))
                {
                    MigrateAndFillData(context);
                }
                break;
            case XpoProvider.SQLite:
                using (var context = new SqliteMigrationContext(builder.Options, _appSettingsOptions))
                {
                    MigrateAndFillData(context);
                }
                break;
            case XpoProvider.MSSqlServer:
                using (var context = new MsSqlServerMigrationContext(builder.Options, _appSettingsOptions))
                {
                    MigrateAndFillData(context);
                }
                break;
            case XpoProvider.InMemoryDataStore:
                break;
            default:
                throw new InvalidOperationException("Unknown provider");
        }
    }

    private void MigrateAndFillData(AppSettingsContext context)
    {
        context.Database.Migrate();
        CreateAndSaveDefaultValues(context);
        Data = context.AppSettings.ToDictionary(c => c.Key, c => c.Value)!;
    }

    /// <summary>
    /// Create and save settings with default value to db
    /// </summary>
    /// <param name="dbContext"></param>
    private void CreateAndSaveDefaultValues(AppSettingsContext dbContext)
    {
        var configValues =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        ToDictionary(configValues, _settings, string.Empty);
        var list = dbContext.AppSettings.ToList();

        foreach (var keyValue in configValues)
        {
            if (list.Exists(x => x.Key == keyValue.Key))
                continue;
            dbContext.AppSettings.Add(new AppSettingEntity
            {
                Key = keyValue.Key,
                Value = keyValue.Value
            });
        }

        dbContext.SaveChanges();
    }

    /// <summary>
    /// Convert application settings instance to dictionary
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="obj"></param>
    /// <param name="prefix"></param>
    private static void ToDictionary(Dictionary<string, string> dictionary, object obj, string prefix)
    {
        var fields = obj.GetType().GetProperties();
        foreach (var field in fields)
        {
            if (field.GetCustomAttribute(typeof(NotSaveToDbAttribute)) != null)
                continue;

            var value = field.GetValue(obj);
            if (value != null)
            {
                var key = string.IsNullOrEmpty(prefix)
                    ? field.Name
                    : string.Join(':', prefix, field.Name);
                if (value is string or int or double or bool)
                {
                    dictionary.Add(key, value.ToString()!);
                }
                else if (value.GetType().IsGenericType)
                {
                    GenericToDictionary(dictionary, prefix, value, key);
                }
                else
                {
                    ToDictionary(dictionary, value, key);
                }
            }
        }
    }

    /// <summary>
    /// Generic type to dictionary
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="prefix"></param>
    /// <param name="value"></param>
    /// <param name="key"></param>
    private static void GenericToDictionary(Dictionary<string, string> dictionary, string prefix, object value, string key)
    {
        if (value.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            if (value is IDictionary dictionary1)
            {
                foreach (DictionaryEntry keyValue in dictionary1)
                {
                    var key2 = prefix + ":" + keyValue.Key;
                    dictionary.Add(key2, keyValue.Value?.ToString()!);
                }
            }
        }
        else if (value.GetType().GetGenericTypeDefinition() == typeof(List<>))
        {
            int i = 0;
            foreach (var item in (IEnumerable)value)
            {
                ToDictionary(dictionary, item, $"{key}:{i}");
                i++;
            }
        }
    }
}