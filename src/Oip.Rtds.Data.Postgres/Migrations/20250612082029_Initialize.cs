using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Oip.Rtds.Data.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Initialize : Migration
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
                    Source = table.Column<string>(type: "text", nullable: true),
                    Descriptor = table.Column<string>(type: "text", nullable: true),
                    EngUnits = table.Column<string>(type: "text", nullable: true),
                    InstrumentTag = table.Column<string>(type: "text", nullable: true),
                    Archiving = table.Column<bool>(type: "boolean", nullable: true),
                    Compressing = table.Column<bool>(type: "boolean", nullable: true),
                    ExcDev = table.Column<double>(type: "double precision", nullable: true),
                    ExcMin = table.Column<int>(type: "integer", nullable: true),
                    ExcMax = table.Column<int>(type: "integer", nullable: true),
                    CompDev = table.Column<double>(type: "double precision", nullable: true),
                    CompMin = table.Column<int>(type: "integer", nullable: true),
                    CompMax = table.Column<int>(type: "integer", nullable: true),
                    Zero = table.Column<int>(type: "integer", nullable: true),
                    Span = table.Column<int>(type: "integer", nullable: true),
                    Location1 = table.Column<int>(type: "integer", nullable: true),
                    Location2 = table.Column<int>(type: "integer", nullable: true),
                    Location3 = table.Column<int>(type: "integer", nullable: true),
                    Location4 = table.Column<int>(type: "integer", nullable: true),
                    Location5 = table.Column<int>(type: "integer", nullable: true),
                    ExDesc = table.Column<string>(type: "text", nullable: true),
                    Scan = table.Column<bool>(type: "boolean", nullable: true),
                    DigitalSet = table.Column<string>(type: "text", nullable: true),
                    Step = table.Column<bool>(type: "boolean", nullable: true),
                    Future = table.Column<bool>(type: "boolean", nullable: true),
                    UserInt1 = table.Column<int>(type: "integer", nullable: true),
                    UserInt2 = table.Column<int>(type: "integer", nullable: true),
                    UserInt3 = table.Column<int>(type: "integer", nullable: true),
                    UserInt4 = table.Column<int>(type: "integer", nullable: true),
                    UserInt5 = table.Column<int>(type: "integer", nullable: true),
                    UserReal1 = table.Column<double>(type: "double precision", nullable: true),
                    UserReal2 = table.Column<double>(type: "double precision", nullable: true),
                    UserReal3 = table.Column<double>(type: "double precision", nullable: true),
                    UserReal4 = table.Column<double>(type: "double precision", nullable: true),
                    UserReal5 = table.Column<double>(type: "double precision", nullable: true),
                    CreationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Creator = table.Column<string>(type: "text", nullable: true),
                    Partition = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.TagId);
                });
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
