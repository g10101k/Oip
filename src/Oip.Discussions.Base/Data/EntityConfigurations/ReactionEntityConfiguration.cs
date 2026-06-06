using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;

namespace Oip.Discussions.Data.EntityConfigurations;

/// <summary>
/// Configuration for the ReactionEntity model.
/// </summary>
public class ReactionEntityConfiguration(DatabaseFacade database, bool designTime, string schemaName)
    : IEntityTypeConfiguration<ReactionEntity>
{
    /// <summary>
    /// Configures the ReactionEntity model.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<ReactionEntity> builder)
    {
        if (designTime)
            Console.Write($"Configuring {nameof(ReactionEntityConfiguration)}");

        builder.SetTableWithSchema(database, schemaName);
        builder.HasKey(e => e.ReactionId);
        builder.Property(e => e.ReactionId).ValueGeneratedOnAdd();
        builder.Property(e => e.CommentId).IsRequired();
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.EmojiCode).IsRequired().HasMaxLength(20);
        builder.Property(e => e.ReactedAt).IsRequired();
        builder.HasIndex(e => new { e.CommentId, e.UserId }).IsUnique();
        builder.HasIndex(e => e.CommentId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.EmojiCode);

        // Relationship with CommentEntity
        builder.HasOne(e => e.Comment)
            .WithMany(c => c.Reactions)
            .HasForeignKey(e => e.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}