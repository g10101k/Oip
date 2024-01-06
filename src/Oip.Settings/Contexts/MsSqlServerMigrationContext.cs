using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Oip.Settings.Migrations;

namespace Oip.Settings.Contexts;

internal class MsSqlServerMigrationContext : AppSettingsContext
{
    public MsSqlServerMigrationContext(DbContextOptions<AppSettingsContext> options, AppSettingsOptions appSettingsOptions) :
        base(options, appSettingsOptions)
    {
    }
}

// ReSharper disable once UnusedType.Global
internal class AppSettingsContextFactory : IDesignTimeDbContextFactory<MsSqlServerMigrationContext>
{
    public MsSqlServerMigrationContext CreateDbContext(string[] args)
    {
        var settings = AppSettings.Initialize(args);
        var optionsBuilder = new DbContextOptionsBuilder<AppSettingsContext>();
        settings.AppSettingsOptions.Builder(optionsBuilder, settings.Provider, settings.NormalizedConnectionString);
        return new MsSqlServerMigrationContext(optionsBuilder.Options, AppSettings.Instance.AppSettingsOptions);
    }
}
