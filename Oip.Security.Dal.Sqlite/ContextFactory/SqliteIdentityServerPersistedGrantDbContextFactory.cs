using System.Linq;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Oip.Security.Dal.DbContexts;

namespace Oip.Security.Dal.Sqlite.ContextFactory;

// ReSharper disable once UnusedType.Global
public class SqliteIdentityServerPersistedGrantDbContextFactory :
    IDesignTimeDbContextFactory<IdentityServerPersistedGrantDbContext>
{
    public IdentityServerPersistedGrantDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<IdentityServerPersistedGrantDbContext>();
        var connectionString = args.Any() ? args[0] : Constants.DefaultConnectionString;
        builder.UseSqlite(connectionString, db => db
            .MigrationsAssembly(typeof(SqliteBaseDbContextFactory<>).Assembly.GetName().Name)
            .MigrationsHistoryTable(Constants.MigrationsHistoryTable));
        return new IdentityServerPersistedGrantDbContext(builder.Options, new OperationalStoreOptions());
    }
}