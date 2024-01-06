using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oip.Settings.Entities;

namespace Oip.Settings.EntityConfigurations;

/// <inheritdoc />
public class AppSettingConfiguration : IEntityTypeConfiguration<AppSettingEntity>
{
    private readonly string _tableName;
    private readonly string _schema;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="schema"></param>
    public AppSettingConfiguration(string tableName, string schema)
    {
        _tableName = tableName;
        _schema = schema;
    }

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AppSettingEntity> builder)
    {
        builder.ToTable(_tableName, _schema);
        builder.HasKey(e => e.Key);

        builder.Property(e => e.Key)
            .HasMaxLength(512);
    }
}