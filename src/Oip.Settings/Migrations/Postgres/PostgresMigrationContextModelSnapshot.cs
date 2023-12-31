﻿// <auto-generated />

#nullable disable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Oip.Settings.Contexts;

namespace Oip.Settings.Migrations.Postgres
{
    [DbContext(typeof(PostgresMigrationContext))]
    partial class PostgresMigrationContextModelSnapshot : SettingsModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("settings")
                .HasAnnotation("ProductVersion", "6.0.22")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Oip.Settings.Entities.AppSettingEntity", b =>
                {
                    b.Property<string>("Key")
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Key");

                    b.ToTable(AppSettingsTable, AppSettingsSchema);
                });
#pragma warning restore 612, 618
        }
    }
}
