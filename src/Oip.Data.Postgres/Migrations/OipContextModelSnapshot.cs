﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Oip.Data.Contexts;

#nullable disable

namespace Oip.Data.Postgres.Migrations
{
    [DbContext(typeof(OipContext))]
    partial class OipContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Oip.Data.Entities.FeatureEntity", b =>
                {
                    b.Property<int>("FeatureId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("FeatureId"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<string>("Settings")
                        .HasColumnType("text");

                    b.HasKey("FeatureId");

                    b.ToTable("Features");
                });
#pragma warning restore 612, 618
        }
    }
}
