using System.Reflection;
using IdentityServer4.EntityFramework.Storage;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oip.Security.Dal.Interfaces;
using Skoruba.AuditLogging.EntityFramework.DbContexts;
using Skoruba.AuditLogging.EntityFramework.Entities;

namespace Oip.Security.Dal.Configuration.PostgreSQL;

public static class DatabaseExtensions
{
    /// <summary>
    ///     Register DbContexts for IdentityServer ConfigurationStore and PersistedGrants, Identity and Logging
    ///     Configure the connection strings in AppSettings.json
    /// </summary>
    /// <typeparam name="TConfigurationDbContext"></typeparam>
    /// <typeparam name="TPersistedGrantDbContext"></typeparam>
    /// <typeparam name="TLogDbContext"></typeparam>
    /// <typeparam name="TIdentityDbContext"></typeparam>
    /// <typeparam name="TAuditLoggingDbContext"></typeparam>
    /// <typeparam name="TDataProtectionDbContext"></typeparam>
    /// <typeparam name="TAuditLog"></typeparam>
    /// <param name="services"></param>
    /// <param name="connectionStrings"></param>
    /// <param name="databaseMigrations"></param>
    public static void RegisterNpgSqlDbContexts<TIdentityDbContext, TConfigurationDbContext,
        TPersistedGrantDbContext, TLogDbContext, TAuditLoggingDbContext, TDataProtectionDbContext, TAuditLog>(
        this IServiceCollection services,
        ConnectionStringsConfiguration connectionStrings,
        DatabaseMigrationsConfiguration databaseMigrations)
        where TIdentityDbContext : DbContext
        where TPersistedGrantDbContext : DbContext, IAdminPersistedGrantDbContext
        where TConfigurationDbContext : DbContext, IAdminConfigurationDbContext
        where TLogDbContext : DbContext, IAdminLogDbContext
        where TAuditLoggingDbContext : DbContext, IAuditLoggingDbContext<TAuditLog>
        where TDataProtectionDbContext : DbContext, IDataProtectionKeyContext
        where TAuditLog : AuditLog
    {
        var migrationsAssembly = typeof(DatabaseExtensions).GetTypeInfo().Assembly.GetName().Name;

        // Config DB for identity
        services.AddDbContext<TIdentityDbContext>(options =>
            options.UseNpgsql(connectionStrings.ConfigurationDbConnection, sql =>
                sql.MigrationsAssembly(databaseMigrations.IdentityDbMigrationsAssembly ?? migrationsAssembly)));

        // Config DB from existing connection
        services.AddConfigurationDbContext<TConfigurationDbContext>(options =>
            options.ConfigureDbContext = b =>
                b.UseNpgsql(connectionStrings.ConfigurationDbConnection,
                    sql => sql.MigrationsAssembly(databaseMigrations.ConfigurationDbMigrationsAssembly ??
                                                  migrationsAssembly)));

        // Operational DB from existing connection
        services.AddOperationalDbContext<TPersistedGrantDbContext>(options => options.ConfigureDbContext = b =>
            b.UseNpgsql(connectionStrings.ConfigurationDbConnection,
                sql => sql.MigrationsAssembly(databaseMigrations.PersistedGrantDbMigrationsAssembly ??
                                              migrationsAssembly)));

        // Log DB from existing connection
        services.AddDbContext<TLogDbContext>(options => options.UseNpgsql(connectionStrings.ConfigurationDbConnection,
            optionsSql =>
                optionsSql.MigrationsAssembly(databaseMigrations.AdminLogDbMigrationsAssembly ?? migrationsAssembly)));

        // Audit logging connection
        services.AddDbContext<TAuditLoggingDbContext>(options => options.UseNpgsql(
            connectionStrings.ConfigurationDbConnection,
            optionsSql =>
                optionsSql.MigrationsAssembly(
                    databaseMigrations.AdminAuditLogDbMigrationsAssembly ?? migrationsAssembly)));

        // DataProtectionKey DB from existing connection
        if (!string.IsNullOrEmpty(connectionStrings.ConfigurationDbConnection))
            services.AddDbContext<TDataProtectionDbContext>(options =>
                options.UseNpgsql(connectionStrings.ConfigurationDbConnection,
                    sql => sql.MigrationsAssembly(databaseMigrations.DataProtectionDbMigrationsAssembly ??
                                                  migrationsAssembly)));
    }

    /// <summary>
    ///     Register DbContexts for IdentityServer ConfigurationStore and PersistedGrants and Identity
    ///     Configure the connection strings in AppSettings.json
    /// </summary>
    /// <typeparam name="TConfigurationDbContext"></typeparam>
    /// <typeparam name="TPersistedGrantDbContext"></typeparam>
    /// <typeparam name="TIdentityDbContext"></typeparam>
    /// <typeparam name="TDataProtectionDbContext"></typeparam>
    /// <param name="services"></param>
    /// <param name="connectionString"></param>
    public static void RegisterNpgSqlDbContexts<TIdentityDbContext, TConfigurationDbContext,
        TPersistedGrantDbContext, TDataProtectionDbContext>(this IServiceCollection services,
        string connectionString)
        where TIdentityDbContext : DbContext
        where TPersistedGrantDbContext : DbContext, IAdminPersistedGrantDbContext
        where TConfigurationDbContext : DbContext, IAdminConfigurationDbContext
        where TDataProtectionDbContext : DbContext, IDataProtectionKeyContext
    {
        var migrationsAssembly = typeof(DatabaseExtensions).GetTypeInfo().Assembly.GetName().Name;

        // Config DB for identity
        services.AddDbContext<TIdentityDbContext>(options =>
            options.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly)));

        // Config DB from existing connection
        services.AddConfigurationDbContext<TConfigurationDbContext>(options =>
            options.ConfigureDbContext = b =>
                b.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly)));

        // Operational DB from existing connection
        services.AddOperationalDbContext<TPersistedGrantDbContext>(options => options.ConfigureDbContext = b =>
            b.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly)));

        // DataProtectionKey DB from existing connection
        services.AddDbContext<TDataProtectionDbContext>(options =>
            options.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly)));
    }
}