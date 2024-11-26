using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Oip.Data.Contexts;
using Oip.Data.Settings;

namespace Oip.Data.Factories;

// ReSharper disable once UnusedType.Global
internal class OipPostgresMigrationContextFactory : IDesignTimeDbContextFactory<PostgresMigrationContext>
{
    public PostgresMigrationContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OipContext>();
        var settings = DesignDbSettings.Initialize(args, false, true, false);
        optionsBuilder.UseNpgsql(settings.NormalizedConnectionString);
        return new PostgresMigrationContext(optionsBuilder.Options, true);
    }
}