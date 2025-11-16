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
        modelBuilder.ApplyConfiguration(new AppSettingConfiguration(_appSettingsOptions.AppSettingsTable,
            _appSettingsOptions.AppSettingsSchema));
    }


    public void Migrate()
    {
        string sqlFormat;
        string sql;
        if (Database.IsSqlite())
        {
            sqlFormat = """
                        CREATE TABLE IF NOT EXISTS {0}
                        (
                            Key   TEXT not null constraint PK_AppSetting primary key,
                            Value TEXT not null
                        );

                        """;
            sql = string.Format(sqlFormat, _appSettingsOptions.AppSettingsTable);
            Database.ExecuteSqlRaw(sql);
        }
        else if (Database.IsSqlServer())
        {
            sqlFormat = """
                        BEGIN TRY
                            BEGIN TRANSACTION
                            
                            -- Проверка и создание схемы
                            IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{0}')
                            BEGIN
                                EXEC('CREATE SCHEMA [{0}]')
                            END
                            
                            -- Проверка и создание таблицы
                            IF OBJECT_ID('[{0}].[{1}]', 'U') IS NULL
                            BEGIN
                                CREATE TABLE [{0}].[{1}]
                                (
                                    [Key] nvarchar(512) not null constraint PK_{0}_{1} primary key,
                                    [Value] nvarchar(max) not null
                                )
                            END
                            
                            COMMIT TRANSACTION
                        END TRY
                        BEGIN CATCH
                            ROLLBACK TRANSACTION
                            THROW
                        END CATCH
                        """;

            sql = string.Format(sqlFormat, _appSettingsOptions.AppSettingsSchema,
                _appSettingsOptions.AppSettingsTable);
            Database.ExecuteSqlRaw(sql);
        }
        else if (Database.IsInMemory())
        {
            sqlFormat = """
                        CREATE SCHEMA IF NOT EXISTS "{0}";
                        CREATE TABLE IF NOT EXISTS "{0}"."{1}"
                        (
                            "Key" varchar(512) not null primary key,
                            "Value" text not null
                        );
                        """;
            Database.ExecuteSqlRaw(string.Format(sqlFormat, _appSettingsOptions.AppSettingsTable));
        }
    }
}