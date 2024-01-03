using Microsoft.EntityFrameworkCore;
using Oip.Security.Dal.Sqlite.ContextFactory;

namespace Oip.Security.Dal.Sqlite;

// ReSharper disable once UnusedType.Global
public static class DbContextOptionsBuilderExtensions
{
    // ReSharper disable once UnusedMember.Global
    public static DbContextOptionsBuilder UseSqlite(this DbContextOptionsBuilder builder,
        string connectionString = Constants.DefaultConnectionString)
    {
        return builder.UseSqlite(connectionString, db => db
            .MigrationsAssembly(typeof(SqliteBaseDbContextFactory<>).Assembly.GetName().Name)
            .MigrationsHistoryTable(Constants.MigrationsHistoryTable));
    }
}