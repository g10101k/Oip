using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Oip.Data.Contexts;
using Oip.Data.Settings;

namespace Oip.Data.Factories;

// ReSharper disable once UnusedType.Global
internal class OipContextSqliteMigrationFactory : IDesignTimeDbContextFactory<SqliteMigrationContext>
{
    public SqliteMigrationContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OipContext>();
        var settings = DesignDbSettings.Initialize(args, false, true, false);
        optionsBuilder.UseSqlite(settings.NormalizedConnectionString);
        return new SqliteMigrationContext(optionsBuilder.Options, true);
    }
}