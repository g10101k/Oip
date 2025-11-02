using Microsoft.EntityFrameworkCore;
using Oip.Settings.Enums;
using Oip.Settings.Models;

namespace Oip.Settings;

/// <summary>
/// Setting option
/// </summary>
public class AppSettingsOptions
{
    /// <summary>
    /// Program arguments for settings
    /// </summary>
    [Obsolete("Will be removed in further releases. Use \"ProgramArguments\" instead")]
    public string[] ProgrammeArguments
    {
        get => ProgramArguments;
        set => ProgramArguments = value;
    }

    /// <summary>
    /// Program arguments for settings
    /// </summary>
    public string[] ProgramArguments { get; set; } = [];

    /// <summary>
    /// JSON file name
    /// </summary>
    public string JsonFileName { get; set; } = "appsettings.json";

    /// <summary>
    /// JSON file name (development)
    /// </summary>
    public string JsonFileNameDevelopment { get; set; } = "appsettings.Development.json";

    /// <summary>
    /// Exclude migration (table create from root app)
    /// </summary>
    public bool ExcludeMigration { get; set; }

    private string _appSettingsSchema = "settings";

    /// <summary>
    /// Table schema for save settings
    /// </summary>
    public string AppSettingsSchema
    {
        get => _appSettingsSchema;
        set
        {
            _appSettingsSchema = value;
            Environment.SetEnvironmentVariable(nameof(AppSettingsSchema), value);
        }
    }

    private string _appSettingsTable = "AppSetting";

    /// <summary>
    /// Table name for application settings
    /// </summary>
    public string AppSettingsTable
    {
        get => _appSettingsTable;
        set
        {
            _appSettingsTable = value;
            Environment.SetEnvironmentVariable(nameof(AppSettingsTable), value);
        }
    }

    /// <summary>
    /// DbContextOptionsBuilder
    /// </summary>
    public Action<DbContextOptionsBuilder, XpoProvider, string> Builder { get; set; } =
        (option, provider, connection) =>
        {
            var defaultSchema = "settings";
            var migrationTableName = "AppSettingsMigrationsHistory";
            switch (provider)
            {
                case XpoProvider.SQLite:
                    option.UseSqlite(connection, builder => { builder.MigrationsHistoryTable(migrationTableName); });
                    break;
                case XpoProvider.Postgres:
                    option.UseNpgsql(connection,
                        builder => { builder.MigrationsHistoryTable(migrationTableName, defaultSchema); });
                    break;
                case XpoProvider.MSSqlServer:
                    option.UseSqlServer(connection,
                        builder => { builder.MigrationsHistoryTable(migrationTableName, defaultSchema); });
                    break;
                default:
                    option.UseInMemoryDatabase(connection);
                    break;
            }
        };

    /// <summary>
    /// Use EFCore settings provider, default - true
    /// </summary>
    public bool UseEfCoreProvider { get; set; } = true;

    /// <summary>
    /// Normalize connection string
    /// </summary>
    public bool NormalizeConnectionString { get; set; } = true;
}