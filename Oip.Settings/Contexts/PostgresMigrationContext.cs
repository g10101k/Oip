using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Oip.Settings.Migrations;

namespace Oip.Settings.Contexts;

internal class PostgresMigrationContext : AppSettingsContext
{
    public PostgresMigrationContext(DbContextOptions<AppSettingsContext> options, AppSettingsOptions appSettingsOptions) :
        base(options, appSettingsOptions)
    {
    }
}

// ReSharper disable once UnusedType.Global
internal class PostgresMigrationContextFactory : IDesignTimeDbContextFactory<PostgresMigrationContext>
{
    public PostgresMigrationContext CreateDbContext(string[] args)
    {
        var settings = AppSettings.Initialize(args);
        var optionsBuilder = new DbContextOptionsBuilder<AppSettingsContext>();
        settings.AppSettingsOptions.Builder(optionsBuilder, settings.Provider, settings.NormalizedConnectionString);
        return new PostgresMigrationContext(optionsBuilder.Options, AppSettings.Instance.AppSettingsOptions);
    }
}
