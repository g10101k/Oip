using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;

namespace Oip.Discussions.Data.EntityConfigurations;

/// <summary>
/// Configures the CommentEntity for Entity Framework Core.
/// </summary>
/// <param name="database">Database facade instance</param>
/// <param name="designTime">Indicates if configuration is running during design time</param>
/// <param name="schemaName">Database schema name used for table mapping</param>
public class CommentEntityConfiguration(DatabaseFacade database, bool designTime, string schemaName)
    : IEntityTypeConfiguration<CommentEntity>
{
    /// <summary>
    /// Configures the entity mapping and relationships for CommentEntity.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity</param>
    public void Configure(EntityTypeBuilder<CommentEntity> builder)
    {
        if (designTime)
            Console.Write($"Configuring {nameof(CommentEntityConfiguration)}");

        builder.SetTableWithSchema(database, schemaName);

        // Composite primary key
        builder.HasKey(e => new { e.ObjectTypeId, e.ObjectId, e.CommentId });

        // Surrogate key for foreign key relationships
        builder.Property(e => e.CommentId).ValueGeneratedOnAdd();
        builder.HasIndex(e => e.CommentId).IsUnique();

        // Indexes
        builder.HasIndex(e => new { e.ObjectTypeId, e.ObjectId });
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.IsDeleted);

        builder.Property(e => e.ObjectTypeId).IsRequired();
        builder.Property(e => e.ObjectId).IsRequired();
        builder.Property(e => e.CommentId).IsRequired();
        builder.Property(e => e.Content).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.DeletedAt);
        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Navigation properties
        builder.HasMany(e => e.Attachments)
            .WithOne(a => a.Comment)
            .HasForeignKey(a => a.CommentId)
            .HasPrincipalKey(c => c.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.Reactions)
            .WithOne(r => r.Comment)
            .HasForeignKey(r => r.CommentId)
            .HasPrincipalKey(c => c.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.Mentions)
            .WithOne(m => m.Comment)
            .HasForeignKey(m => m.CommentId)
            .HasPrincipalKey(c => c.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.EditHistory)
            .WithOne(h => h.Comment)
            .HasForeignKey(h => h.CommentId)
            .HasPrincipalKey(c => c.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Global filter for soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
