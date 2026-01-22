using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;

namespace Oip.Discussions.Data.EntityConfigurations;

/// <summary>
/// Configures the database mapping for the MentionEntity.
/// </summary>
public class MentionEntityConfiguration(DatabaseFacade database, bool designTime)
    : IEntityTypeConfiguration<MentionEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MentionEntity> builder)
    {
        if (designTime)
            Console.Write($"Configuring {nameof(MentionEntityConfiguration)}");

        builder.SetTableWithSchema(database, DiscussionsDbContext.SchemaName);
        builder.HasKey(e => e.MentionId);
        builder.Property(e => e.MentionId).ValueGeneratedOnAdd();
        builder.Property(e => e.CommentId).IsRequired();
        builder.Property(e => e.MentionedUserId).IsRequired();
        builder.Property(e => e.Position).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.CommentId);
        builder.HasIndex(e => e.MentionedUserId);
        builder.HasIndex(e => new { e.CommentId, e.Position });

        builder.HasOne(e => e.Comment)
            .WithMany()
            .HasForeignKey(e => e.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}