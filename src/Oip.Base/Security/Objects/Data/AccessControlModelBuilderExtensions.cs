using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Oip.Base.Data.Extensions;

namespace Oip.Base.Security.Objects.Data;

/// <summary>
/// Adds the access control tables to the model of a service context.
/// </summary>
public static class AccessControlModelBuilderExtensions
{
    /// <summary>
    /// Maximum length of a subject key.
    /// </summary>
    public const int SubjectKeyMaxLength = 300;

    /// <summary>
    /// Maximum length of an object type or right code.
    /// </summary>
    public const int CodeMaxLength = 128;

    /// <summary>
    /// Configures the access control tables in the given schema of the service.
    /// </summary>
    /// <param name="modelBuilder">Model builder of the service context.</param>
    /// <param name="database">Database facade of the context, used to pick the provider specific mapping.</param>
    /// <param name="schemaName">Schema of the service.</param>
    public static ModelBuilder ApplyOipAccessControl(this ModelBuilder modelBuilder, DatabaseFacade database,
        string schemaName)
    {
        modelBuilder.Entity<AclSetEntity>(entity =>
        {
            entity.SetTableWithSchema(database, schemaName);
            entity.HasKey(e => e.AclSetId);
            entity.Property(e => e.AclSetId).ValueGeneratedOnAdd();
            entity.Property(e => e.Hash).HasMaxLength(64).IsRequired();
            entity.HasIndex(e => e.Hash).IsUnique();
            entity.HasMany(e => e.Entries).WithOne().HasForeignKey(e => e.AclSetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AclSetEntryEntity>(entity =>
        {
            entity.SetTableWithSchema(database, schemaName);
            entity.HasKey(e => new { e.AclSetId, e.SubjectKey });
            entity.Property(e => e.SubjectKey).HasMaxLength(SubjectKeyMaxLength).IsRequired();
            entity.HasIndex(e => new { e.SubjectKey, e.AclSetId });
        });

        modelBuilder.Entity<AccessObjectEntity>(entity =>
        {
            entity.SetTableWithSchema(database, schemaName);
            entity.HasKey(e => new { e.ObjectTypeId, e.ObjectId });
            entity.Property(e => e.ObjectId).ValueGeneratedNever();
            entity.Property(e => e.OwnerSubject).HasMaxLength(SubjectKeyMaxLength);
            entity.HasIndex(e => new { e.ParentTypeId, e.ParentId });
            entity.HasIndex(e => e.AclSetId);
        });

        modelBuilder.Entity<AccessObjectTypeEntity>(entity =>
        {
            entity.SetTableWithSchema(database, schemaName);
            entity.HasKey(e => e.ObjectTypeId);
            entity.Property(e => e.ObjectTypeId).ValueGeneratedOnAdd();
            entity.Property(e => e.Code).HasMaxLength(CodeMaxLength).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<AccessRightEntity>(entity =>
        {
            entity.SetTableWithSchema(database, schemaName);
            entity.HasKey(e => new { e.ObjectTypeId, e.Code });
            entity.Property(e => e.Code).HasMaxLength(CodeMaxLength).IsRequired();
            entity.HasIndex(e => new { e.ObjectTypeId, e.Bit }).IsUnique();
        });

        return modelBuilder;
    }
}
