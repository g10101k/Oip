using System;
using System.Reflection;
using Oip.Security.Dal.Configuration;
using SqlMigrationAssembly = Oip.Security.Dal.SqlServer.Helpers.MigrationAssembly;
using MySqlMigrationAssembly = Oip.Security.Dal.MySql.Helpers.MigrationAssembly;
using PostgreSQLMigrationAssembly = Oip.Security.Dal.PostgreSQL.Helpers.MigrationAssembly;
using SqliteMigrationAssembly = Oip.Security.Dal.Sqlite.Helpers.MigrationAssembly;

namespace Oip.Security.Configuration.Database;

public static class MigrationAssemblyConfiguration
{
    public static string GetMigrationAssemblyByProvider(DatabaseProviderConfiguration databaseProvider)
    {
        return databaseProvider.ProviderType switch
        {
            DatabaseProviderType.SqlServer =>
                GetNameFromAssembly(typeof(SqlMigrationAssembly)),
            DatabaseProviderType.PostgreSql =>
                GetNameFromAssembly(typeof(PostgreSQLMigrationAssembly)),
            DatabaseProviderType.MySql =>
                GetNameFromAssembly(typeof(MySqlMigrationAssembly)),
            DatabaseProviderType.Sqlite =>
                GetNameFromAssembly(typeof(SqliteMigrationAssembly)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static string GetNameFromAssembly(Type type)
    {
        return type.GetTypeInfo().Assembly.GetName().Name;
    }
}