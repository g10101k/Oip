﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Oip.Data.Contexts;

#nullable disable

namespace Oip.Data.Migrations.Postgres
{
    [DbContext(typeof(PostgresMigrationContext))]
    [Migration("20241201132509_InitializePostgres")]
    partial class InitializePostgres
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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

                    b.ToTable("Feature", "oip");
                });

            modelBuilder.Entity("Oip.Data.Entities.FeatureInstanceEntity", b =>
                {
                    b.Property<int>("FeatureId")
                        .HasColumnType("integer");

                    b.Property<int>("FeatureInstanceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("FeatureInstanceId"));

                    b.Property<string>("Settings")
                        .IsRequired()
                        .HasColumnType("text");

                    b.ToTable("FeatureInstance", "oip");
                });

            modelBuilder.Entity("Oip.Data.Entities.FeatureInstanceSecurityEntity", b =>
                {
                    b.Property<int>("FeatureInstanceId")
                        .HasColumnType("integer");

                    b.Property<int>("FeatureInstanceSecurityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("FeatureInstanceSecurityId"));

                    b.Property<string>("Right")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.ToTable("FeatureInstanceSecurity", "oip");
                });

            modelBuilder.Entity("Oip.Data.Entities.FeatureSecurityEntity", b =>
                {
                    b.Property<int>("FeatureId")
                        .HasColumnType("integer");

                    b.Property<int>("FeatureSecurityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("FeatureSecurityId"));

                    b.Property<string>("Right")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.ToTable("FeatureSecurity", "oip");
                });
#pragma warning restore 612, 618
        }
    }
}
