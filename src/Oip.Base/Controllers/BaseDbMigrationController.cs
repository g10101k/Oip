using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Repositories;

namespace Oip.Base.Controllers;

/// <summary>
/// Migration controller
/// </summary>
[ApiController]
public abstract class BaseDbMigrationController<TSettings> : BaseModuleController<TSettings> where TSettings : class
{
    private readonly DbContext _dbContext;

    /// <summary>
    /// .ctor
    /// </summary>
    protected BaseDbMigrationController(ModuleRepository repository, DbContext dbContext) : base(repository)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets the list of migrations.
    /// </summary>
    /// <returns>An enumerable of MigrationDto objects representing the migrations.</returns>
    [Authorize(Roles = "admin")]
    [HttpGet("get-migrations")]
    public async Task<IEnumerable<MigrationDto>> GetMigrations()
    {
        var result = new List<MigrationDto>();
        var allMigrations = _dbContext.Database.GetMigrations();

        foreach (var migration in allMigrations)
        {
            result.Add(new MigrationDto(migration, false, false, true));
        }

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
    /// Migrate database
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = "admin")]
    [HttpGet("migrate-database")]
    public async Task<IActionResult> MigrateDatabase()
    {
        await _dbContext.Database.MigrateAsync();
        return Ok();
    }

    /// <summary>
    /// Applies a specific migration to the database.
    /// </summary>
    /// <param name="request">The request containing the name of the migration to apply.</param>
    /// <returns>An IActionResult representing the result of the operation.</returns>
    [Authorize(Roles = "admin")]
    [HttpPost("apply-migration")]
    public async Task<IActionResult> ApplyMigration(ApplyMigrationRequest request)
    {
        await _dbContext.Database.GetInfrastructure().GetService<IMigrator>()?.MigrateAsync(request.Name)!;
        return Ok();
    }
}

/// <summary>
/// Represents a request to apply a specific migration.
/// </summary>
/// <param name="Name">The name of the migration to apply.</param>
public record ApplyMigrationRequest(string Name);