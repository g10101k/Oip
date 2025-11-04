using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace Oip.Base.Data.Contexts;
#pragma warning disable EF1001

/// <summary>
/// Represents a migration assembly for the user context that handles database migrations for both SQL Server and PostgreSQL.
/// </summary>
public class BaseContextMigrationAssembly<TContextSqlServer, TContextPostgres> : MigrationsAssembly
{
    private readonly IDiagnosticsLogger<DbLoggerCategory.Migrations> _logger;
    private readonly ICurrentDbContext _currentContext;
    private IReadOnlyDictionary<string, TypeInfo>? _migration;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseContextMigrationAssembly{TContextSqlServer,TContextPostgres}"/> class.
    /// </summary>
    /// <param name="currentContext">The current database context.</param>
    /// <param name="options">The database context options.</param>
    /// <param name="idGenerator">The migration ID generator.</param>
    /// <param name="logger">The diagnostics logger for migrations.</param>
    /// <inheritdoc />
    public BaseContextMigrationAssembly(ICurrentDbContext currentContext, IDbContextOptions options,
        IMigrationsIdGenerator idGenerator, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
        : base(currentContext, options, idGenerator, logger)
    {
        _currentContext = currentContext;
        _logger = logger;
    }

    /// <summary>
    /// Gets the collection of migrations for the user context, organized by migration ID.
    /// </summary>
    /// <returns>A dictionary of migration IDs mapped to their corresponding type information.</returns>
    /// <inheritdoc />
    public override IReadOnlyDictionary<string, TypeInfo> Migrations
    {
        get { return _migration ??= Create(); }
    }

    /// <summary>
    /// Creates the migration dictionary by scanning available migrations and organizing them by ID.
    /// </summary>
    /// <returns>A sorted dictionary of migration IDs and their type information.</returns>
    private IReadOnlyDictionary<string, TypeInfo> Create()
    {
        var result = new SortedList<string, TypeInfo>();
        var isSqlServer = _currentContext.Context.Database.IsSqlServer();
        var contextType = isSqlServer ? typeof(TContextSqlServer) : typeof(TContextPostgres);

        var items = from t in Assembly.GetTypes()
            where t.IsSubclassOf(typeof(Migration))
                  && t.GetCustomAttribute<DbContextAttribute>()?.ContextType == contextType
            let id = t.GetCustomAttribute<MigrationAttribute>()?.Id
            orderby id
            select (id, t);

        foreach (var (id, t) in items)
        {
            if (id == null)
            {
                _logger.MigrationAttributeMissingWarning((TypeInfo)t);
                continue;
            }

            result.Add(id, (TypeInfo)t);
        }

        return result;
    }
}
#pragma warning restore EF1001