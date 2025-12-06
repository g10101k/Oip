using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;
using Oip.Users.Contexts;
using Entities_UserEntity = Oip.Users.Entities.UserEntity;
using UserEntity = Oip.Users.Entities.UserEntity;

namespace Oip.Users.EntityConfigurations;

/// <summary>
/// <inheritdoc cref="IEntityTypeConfiguration{TEntity}"/>
/// </summary>
public class UserEntityConfiguration : IEntityTypeConfiguration<Entities_UserEntity>
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
        entity.SetTableWithSchema(_database, UserContext.SchemaName);
        entity.SetPrimaryKey(_designTime, e => e.UserId);
        entity.Property(e => e.UserId).ValueGeneratedOnAdd();
        entity.Property(e => e.Email).HasMaxLength(512);
        entity.Property(e => e.KeycloakId).HasMaxLength(36);
        entity.Property(e=>e.FirstName).HasMaxLength(255);
        entity.Property(e => e.LastName).HasMaxLength(255);
    }
}