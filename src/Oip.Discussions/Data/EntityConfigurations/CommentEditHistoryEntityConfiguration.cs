using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Data.Extensions;

namespace Oip.Discussions.Data.EntityConfigurations;

/// <summary>
/// Configuration for the CommentEditHistoryEntity database mapping.
/// </summary>
/// <param name="database">Database facade instance for schema operations.</param>
/// <param name="designTime">Indicates if configuration is running at design time.</param>
public class CommentEditHistoryEntityConfiguration(DatabaseFacade database, bool designTime, string schemaName)
    : IEntityTypeConfiguration<CommentEditHistoryEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CommentEditHistoryEntity> builder)
    {
        if (designTime)
            Console.Write($"Configuring {nameof(CommentEditHistoryEntity)}");

        builder.SetTableWithSchema(database, schemaName);

        builder.HasKey(e => e.CommentEditHistoryId);
        builder.Property(e => e.CommentEditHistoryId).ValueGeneratedOnAdd();
        builder.Property(e => e.CommentId).IsRequired();
        builder.Property(e => e.OldContent).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.NewContent).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.EditedByUserId).IsRequired();
        builder.Property(e => e.EditedAt).IsRequired();

        builder.HasIndex(e => e.CommentId);
        builder.HasIndex(e => e.EditedByUserId);
        builder.HasIndex(e => e.EditedAt);

        builder.HasOne(e => e.Comment)
            .WithMany()
            .HasForeignKey(e => e.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}