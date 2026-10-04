using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Oip.Base.Data.Contexts;
using Oip.Base.Data.Extensions;
using Oip.Hitl.Data.Entities;
using Oip.Hitl.Data.EntityConfigurations;

namespace Oip.Hitl.Data.Contexts;

/// <summary>
/// EF Core context for LLM providers, agents and their skills. Lives in the <c>llm</c> schema of the application database.
/// </summary>
/// <param name="options">The options for this context</param>
/// <param name="designTime">Whether this context is being used at design time</param>
public class LlmContext(DbContextOptions<LlmContext> options, bool designTime = false) : DbContext(options)
{
    /// <summary>
    /// Database schema used by this context.
    /// </summary>
    public const string SchemaName = "llm";

    /// <summary>
    /// Migration history table for this context.
    /// </summary>
    public const string MigrationHistoryTableName = "__LlmMigrationHistory";

    /// <summary>
    /// Configured LLM providers.
    /// </summary>
    public DbSet<LlmProviderEntity> Providers => Set<LlmProviderEntity>();

    /// <summary>
    /// Agents exposed to chat UIs.
    /// </summary>
    public DbSet<AgentEntity> Agents => Set<AgentEntity>();

    /// <summary>
    /// Skills of agents.
    /// </summary>
    public DbSet<SkillEntity> Skills => Set<SkillEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new LlmProviderEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new AgentEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new SkillEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new AgentSkillEntityConfiguration(Database));
        modelBuilder.ApplyConfiguration(new SkillToolEntityConfiguration(Database));

        modelBuilder.ApplyXmlDocumentation(designTime);
    }

    /// <summary>
    /// Configures the context options by replacing the migration assembly service
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the options builder is not properly configured
    /// </exception>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .ReplaceService<IMigrationsAssembly, BaseContextMigrationAssembly<LlmContextSqlServer, LlmContextPostgres>>();
        if (!optionsBuilder.IsConfigured)
            throw new InvalidOperationException("OnConfiguring error");
    }
}

/// <summary>
/// Represents the SQL Server database context for LLM provider entities
/// </summary>
/// <param name="options">The options for this context</param>
/// <param name="designTime">Whether this context is being used at design time</param>
public class LlmContextSqlServer(DbContextOptions<LlmContext> options, bool designTime = true)
    : LlmContext(options, designTime);

/// <summary>
/// Represents the PostgreSQL database context for LLM provider entities
/// </summary>
/// <param name="options">The options for this context</param>
/// <param name="designTime">Whether this context is being used at design time</param>
public class LlmContextPostgres(DbContextOptions<LlmContext> options, bool designTime = true)
    : LlmContext(options, designTime);
