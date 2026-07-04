using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;
using Oip.Users.Base.Contexts;
using Oip.Users.Base.Data.Entities;

namespace Oip.Users.Base.Data.EntityConfigurations;

internal class UserExtensionEntityConfiguration(DatabaseFacade database, bool designTime = false)
    : IEntityTypeConfiguration<UserExtensionEntity>
{
    public void Configure(EntityTypeBuilder<UserExtensionEntity> entity)
    {
        entity.SetTableWithSchema(database, UserContext.SchemaName);
        entity.SetPrimaryKey(designTime, e => e.UserId);
        entity.Property(x => x.UserId).ValueGeneratedNever();
        if (designTime)
        {
            entity.Ignore(e => e.User);
        }
        else
        {
            entity.HasOne(x => x.User)
                .WithOne(x => x.Extension)
                .HasForeignKey<UserExtensionEntity>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}