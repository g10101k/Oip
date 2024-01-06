using Microsoft.EntityFrameworkCore;
using Oip.Settings.Entities;
using Oip.Settings.EntityConfigurations;

namespace Oip.Settings.Contexts;

/// <summary>
/// Data context
/// </summary>
public class AppSettingsContext : DbContext
{
    private readonly AppSettingsOptions _appSettingsOptions;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="appSettingsOptions"></param>
    public AppSettingsContext(DbContextOptions<AppSettingsContext> options, AppSettingsOptions appSettingsOptions) :
        base(options)
    {
        _appSettingsOptions = appSettingsOptions;
    }

    /// <summary>
    /// Application settings DbSet
    /// </summary>
    public DbSet<AppSettingEntity> AppSettings => Set<AppSettingEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (!Database.IsSqlite() && !Database.IsInMemory())
            modelBuilder.HasDefaultSchema(_appSettingsOptions.AppSettingsSchema);
        modelBuilder.ApplyConfiguration(new AppSettingConfiguration(_appSettingsOptions.AppSettingsTable, _appSettingsOptions.AppSettingsSchema));
    }
}