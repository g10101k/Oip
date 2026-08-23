using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Oip.Base.Data.Contexts;
using Oip.Base.Data.Settings;
using Oip.Settings.Enums;

namespace Oip.Base.Data.Factories;

// ReSharper disable once UnusedType.Global
internal class OipModuleContextDesignTimeDbContextFactory : IDesignTimeDbContextFactory<OipModuleContext>
{
    public OipModuleContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OipModuleContext>();
        var settings = DesignDbSettings.Initialize(args, false, true);
        var model = settings.ConnectionString;
        switch (model.Provider)
        {
            case XpoProvider.Postgres:
                optionsBuilder.UseNpgsql(model.NormalizeConnectionString,
                    x => x.MigrationsHistoryTable(OipModuleContext.MigrationHistoryTableName,
                        OipModuleContext.SchemaName));
                break;
            case XpoProvider.MSSqlServer:
                optionsBuilder.UseSqlServer(model.NormalizeConnectionString,
                    x => x.MigrationsHistoryTable(OipModuleContext.MigrationHistoryTableName,
                        OipModuleContext.SchemaName));
                break;
            default:
                throw new InvalidOperationException($"Provider `{Enum.GetName(model.Provider)}` is not supported");
        }

        optionsBuilder.EnableSensitiveDataLogging(model.SensitiveDataLogging);

        return new OipModuleContext(optionsBuilder.Options, true);
    }
}
