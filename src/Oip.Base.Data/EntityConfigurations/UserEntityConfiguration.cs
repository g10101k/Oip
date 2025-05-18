using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Base.Data.Entities;
using Oip.Base.Data.Extensions;

namespace Oip.Base.Data.EntityConfigurations;

/// <summary>
/// <inheritdoc cref="IEntityTypeConfiguration{TEntity}"/>
/// </summary>
public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    private readonly DatabaseFacade _database;
    private readonly bool _designTime;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="database"></param>
    /// <param name="designTime"></param>
    public UserEntityConfiguration(DatabaseFacade database, bool designTime = false)
    {
        _database = database;
        _designTime = designTime;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="entity"></param>
    public void Configure(EntityTypeBuilder<UserEntity> entity)
    {
        entity.SetTable(_database);
        entity.SetPrimaryKey(_designTime, e => e.UserId);
        entity.Property(e => e.UserId).ValueGeneratedOnAdd();
        entity.Property(e => e.Email).HasMaxLength(512);
    }
}