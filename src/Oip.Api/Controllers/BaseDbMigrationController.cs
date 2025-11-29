using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Repositories;
using Oip.Base.Exceptions;

namespace Oip.Api.Controllers;

/// <summary>
/// Base controller for managing database migrations.
/// </summary>
/// <remarks>
/// Provides administrative endpoints to inspect and manage Entity Framework Core database migrations.
/// Includes functionality to list all migrations, apply pending ones, or migrate to a specific version.
/// Intended to be inherited and used in modules that expose migration endpoints.
/// </remarks>
[ApiController]
public abstract class BaseDbMigrationController<TSettings> : BaseModuleController<TSettings> where TSettings : class
{
    private readonly DbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDbMigrationController{TSettings}"/> class.
    /// </summary>
    /// <param name="repository">Module repository instance used for module-specific operations.</param>
    /// <param name="dbContext">Entity Framework database context used for migration operations.</param>
    protected BaseDbMigrationController(ModuleRepository repository, DbContext dbContext) : base(repository)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Retrieves all database migrations and their current state.
    /// </summary>
    /// <remarks>
    /// The returned list includes:
    /// - Migrations that have been applied to the database.
    /// - Pending migrations that exist in code but are not applied.
    /// - Migrations defined in code regardless of their application status.
    /// </remarks>
    /// <returns>A list of <see cref="MigrationDto"/> objects containing migration metadata.</returns>
    [HttpGet("get-migrations")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType<IEnumerable<MigrationDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<OipException>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<OipException>(StatusCodes.Status500InternalServerError)]
    public virtual async Task<IEnumerable<MigrationDto>> GetMigrations()
    {
        var allMigrations = _dbContext.Database.GetMigrations();

        var result = allMigrations.Select(migration => new MigrationDto(migration, false, false, true)).ToList();

        var appliedMigrations = await _dbContext.Database.GetAppliedMigrationsAsync();
        foreach (var migration in appliedMigrations)
        {
            var m = result.Find(x => x.Name == migration);
            if (m != null)
            {
                m.Applied = true;
            }
            else
                result.Add(new MigrationDto(migration, true, false, false));
        }

        var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
        foreach (var migration in pendingMigrations)
        {
            var m = result.Find(x => x.Name == migration);
            if (m != null)
            {
                m.Pending = true;
            }
            else
                result.Add(new MigrationDto(migration, false, true, false));
        }

        return result;
    }

    /// <summary>
    /// Applies all pending migrations to the database.
    /// </summary>
    /// <remarks>
    /// Uses Entity Framework Core's migration mechanism to bring the database schema
    /// up to date with the current codebase. This operation is irreversible and should be
    /// performed with caution in production environments.
    /// </remarks>
    /// <returns>HTTP 200 OK on success.</returns>
    [HttpPost("migrate")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<OipException>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<OipException>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAppliedMigrations()
    {
        await _dbContext.Database.MigrateAsync();
        return Ok();
    }

    /// <summary>
    /// Applies a specific database migration by name.
    /// </summary>
    /// <remarks>
    /// This method allows applying or reverting to a specific migration by name.
    /// Useful for targeted database updates or rolling back schema changes.
    /// </remarks>
    /// <param name="request">A request containing the name of the migration to apply.</param>
    /// <returns>HTTP 200 OK on success.</returns>
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [HttpPost("apply-migration")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<OipException>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<OipException>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ApplyMigration(ApplyMigrationRequest request)
    {
        await _dbContext.Database.GetInfrastructure().GetService<IMigrator>()?.MigrateAsync(request.Name)!;
        return Ok();
    }
}

/// <summary>
/// Data transfer object representing a database migration and its status.
/// </summary>
public class MigrationDto(string name, bool applied, bool pending, bool exist)
{
    /// <summary>
    /// Name of the migration.
    /// </summary>
    public string Name { get; set; } = name;

    /// <summary>
    /// Indicates whether the migration has been applied.
    /// </summary>
    public bool Applied { get; set; } = applied;

    /// <summary>
    /// Indicates whether the migration is pending.
    /// </summary>
    public bool Pending { get; set; } = pending;

    /// <summary>
    /// Indicates whether the migration exists in the codebase.
    /// </summary>
    public bool Exist { get; set; } = exist;
}

/// <summary>
/// Request model for applying a specific migration by name.
/// </summary>
/// <param name="Name">The name of the migration to apply.</param>
public record ApplyMigrationRequest(string Name);