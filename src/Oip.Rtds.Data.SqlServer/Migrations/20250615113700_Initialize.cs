using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Rtds.SqlServer.Migrations
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
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ValueType = table.Column<int>(type: "int", nullable: false),
                    Interface = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Descriptor = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Uom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstrumentTag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Compressing = table.Column<bool>(type: "bit", nullable: false),
                    CompressionMinTime = table.Column<long>(type: "bigint", nullable: true),
                    CompressionMaxTime = table.Column<long>(type: "bigint", nullable: true),
                    Zero = table.Column<double>(type: "float", nullable: false),
                    Span = table.Column<double>(type: "float", nullable: false),
                    Scan = table.Column<bool>(type: "bit", nullable: true),
                    DigitalSet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Step = table.Column<bool>(type: "bit", nullable: false),
                    TimeCalculation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorCalculation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueCaclulation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Creator = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Partition = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
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
