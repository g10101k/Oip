using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
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
    /// Get migration
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = "admin")]
    [HttpGet("get-migrations")]
    public virtual async Task<IEnumerable<MigrationDto>> GetMigrations()
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
    /// Применить миграцию БД
    /// </summary>
    /// <returns></returns>
    [Authorize(Roles = "admin")]
    [HttpGet("migrate")]
    public async Task<IActionResult> GetAppliedMigrations()
    {
        await _dbContext.Database.MigrateAsync();
        return Ok();
    }

    /// <summary>
    /// Применить миграцию БД
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Roles = "admin")]
    [HttpPost("apply-migration")]
    public async Task<IActionResult> ApplyMigration(ApplyMigrationRequest request)
    {
        await _dbContext.Database.GetInfrastructure().GetService<IMigrator>()?.MigrateAsync(request.Name)!;
        return Ok();
    }
}

/// <summary>
/// Модель миграции
/// </summary>
public class MigrationDto(string name, bool applied, bool pending, bool exist)
{
    /// <summary></summary>
    public string Name { get; set; } = name;

    /// <summary></summary>
    public bool Applied { get; set; } = applied;

    /// <summary></summary>
    public bool Pending { get; set; } = pending;

    /// <summary></summary>
    public bool Exist { get; set; } = exist;
}

/// <summary>
/// Apply Migration Request
/// </summary>
/// <param name="Name">Migration name</param>
public record ApplyMigrationRequest(string Name);