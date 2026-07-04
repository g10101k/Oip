using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;
using Oip.Users.Base.Contexts;
using Entities_UserEntity = Oip.Users.Base.Data.Entities.UserEntity;

namespace Oip.Users.Base.Data.EntityConfigurations;

/// <summary>
/// <inheritdoc cref="IEntityTypeConfiguration{TEntity}"/>
/// </summary>
public class UserEntityConfiguration(DatabaseFacade database, bool designTime = false)
    : IEntityTypeConfiguration<Entities_UserEntity>
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="entity"></param>
    public void Configure(EntityTypeBuilder<Entities_UserEntity> entity)
    {
        entity.SetTableWithSchema(database, UserContext.SchemaName);
        entity.SetPrimaryKey(designTime, e => e.UserId);
        entity.Property(e => e.UserId).ValueGeneratedOnAdd();
        entity.Property(e => e.Email).HasMaxLength(512);
        entity.Property(e => e.KeycloakId).HasMaxLength(36);
        entity.Property(e => e.FirstName).HasMaxLength(255);
        entity.Property(e => e.LastName).HasMaxLength(255);
        entity.Property(e => e.PhotoObjectName).HasMaxLength(512);
        entity.Property(e => e.PhotoContentType).HasMaxLength(128);
        if (designTime)
        {
            entity.Ignore(e => e.Extension);
        }
        
    }
}