using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;

namespace Oip.Discussions.Data.EntityConfigurations;

/// <summary>
/// Configuration for the AttachmentEntity database mapping.
/// </summary>
/// <param name="database">Database facade instance for schema operations.</param>
/// <param name="designTime">Indicates if configuration is running at design time.</param>
public class AttachmentEntityConfiguration(DatabaseFacade database, bool designTime)
    : IEntityTypeConfiguration<AttachmentEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AttachmentEntity> builder)
    {
        if (designTime)
            Console.Write($"Configuring {nameof(AttachmentEntityConfiguration)}");

        builder.SetTableWithSchema(database, DiscussionsDbContext.SchemaName);
        builder.HasKey(e => e.AttachmentId);
        builder.Property(e => e.AttachmentId).ValueGeneratedOnAdd();
        builder.Property(e => e.CommentId).IsRequired();
        builder.Property(e => e.FileName).IsRequired().HasMaxLength(255);
        builder.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
        builder.Property(e => e.FileType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.FileSize).IsRequired();
        builder.Property(e => e.UploadedAt).IsRequired();
        builder.Property(e => e.StorageFileId).IsRequired();

        builder.HasIndex(e => e.CommentId);
        builder.HasIndex(e => e.StorageFileId).IsUnique();

        builder.HasOne(e => e.Comment)
            .WithMany(c => c.Attachments)
            .HasForeignKey(e => e.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}