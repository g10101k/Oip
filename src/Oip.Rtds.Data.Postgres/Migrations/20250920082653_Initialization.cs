using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Oip.Rtds.Data.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Initialization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "rtds");

            migrationBuilder.CreateTable(
                name: "Tag",
                schema: "rtds",
                columns: table => new
                {
                    TagId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ValueType = table.Column<int>(type: "integer", nullable: false),
                    Interface = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Descriptor = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Uom = table.Column<string>(type: "text", nullable: true),
                    InstrumentTag = table.Column<string>(type: "text", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Compressing = table.Column<bool>(type: "boolean", nullable: false),
                    CompressionMinTime = table.Column<long>(type: "bigint", nullable: true),
                    CompressionMaxTime = table.Column<long>(type: "bigint", nullable: true),
                    Zero = table.Column<double>(type: "double precision", nullable: false),
                    Span = table.Column<double>(type: "double precision", nullable: false),
                    Scan = table.Column<bool>(type: "boolean", nullable: true),
                    DigitalSet = table.Column<string>(type: "text", nullable: true),
                    Step = table.Column<bool>(type: "boolean", nullable: false),
                    TimeCalculation = table.Column<string>(type: "text", nullable: true),
                    ErrorCalculation = table.Column<string>(type: "text", nullable: true),
                    ValueCalculation = table.Column<string>(type: "text", nullable: true),
                    CreationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Creator = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Partition = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.TagId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tag_Name",
                schema: "rtds",
                table: "Tag",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tag",
                schema: "rtds");
        }
    }
}
