using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Oip.Settings.Migrations;

namespace Oip.Settings.Contexts;

internal class SqliteMigrationContext : AppSettingsContext
{
    public SqliteMigrationContext(DbContextOptions<AppSettingsContext> options, AppSettingsOptions appSettingsOptions) :
        base(options, appSettingsOptions)
    {
    }
}

// ReSharper disable once UnusedType.Global
internal class SqliteMigrationContextFactory : IDesignTimeDbContextFactory<SqliteMigrationContext>
{
    public SqliteMigrationContext CreateDbContext(string[] args)
    {
        var settings = AppSettings.Initialize(args);
        var optionsBuilder = new DbContextOptionsBuilder<AppSettingsContext>();
        settings.AppSettingsOptions.Builder(optionsBuilder, settings.Provider, settings.NormalizedConnectionString);
        return new SqliteMigrationContext(optionsBuilder.Options, AppSettings.Instance.AppSettingsOptions);
    }
}
