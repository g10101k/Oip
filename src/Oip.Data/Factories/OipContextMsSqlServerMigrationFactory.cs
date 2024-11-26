using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Oip.Data.Contexts;
using Oip.Data.Settings;

namespace Oip.Data.Factories;

// ReSharper disable once UnusedType.Global
internal class OipContextMsSqlServerMigrationFactory : IDesignTimeDbContextFactory<MsSqlServerMigrationContext>
{
    public MsSqlServerMigrationContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OipContext>();
        var settings = DesignDbSettings.Initialize(args, false, true, false);
        optionsBuilder.UseSqlServer(settings.NormalizedConnectionString);
        return new MsSqlServerMigrationContext(optionsBuilder.Options, true);
    }
}