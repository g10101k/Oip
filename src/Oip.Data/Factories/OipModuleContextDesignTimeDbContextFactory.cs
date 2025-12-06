using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Oip.Data.Contexts;
using Oip.Data.Settings;
using Oip.Settings.Enums;

namespace Oip.Data.Factories;

// ReSharper disable once UnusedType.Global
internal class OipModuleContextDesignTimeDbContextFactory : IDesignTimeDbContextFactory<OipModuleContext>
{
    public OipModuleContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OipModuleContext>();
        var settings = DesignDbSettings.Initialize(args, false, true);
        switch (settings.Provider)
        {
            case XpoProvider.Postgres:
                optionsBuilder.UseNpgsql(settings.NormalizedConnectionString, x =>
                {
                    x.MigrationsAssembly("Oip.Data.Postgres");
                });
                break;
            case XpoProvider.MSSqlServer:
                optionsBuilder.UseSqlServer(settings.NormalizedConnectionString, x =>
                {
                    x.MigrationsAssembly("Oip.Data.SqlServer");
                });
                break;
            default:
                throw new InvalidOperationException($"Provider `{Enum.GetName(settings.Provider)}` is not supported");
        }

        return new OipModuleContext(optionsBuilder.Options, true);
    }
}