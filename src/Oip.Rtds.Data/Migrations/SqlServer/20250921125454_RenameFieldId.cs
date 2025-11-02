#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Oip.Rtds.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class RenameFieldId : Migration
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
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ValueType = table.Column<int>(type: "int", nullable: false),
                    Interface = table.Column<long>(type: "bigint", maxLength: 128, nullable: false),
                    Descriptor = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Uom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstrumentTag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    Compressing = table.Column<bool>(type: "bit", nullable: false),
                    CompressionMinTime = table.Column<long>(type: "bigint", nullable: true),
                    CompressionMaxTime = table.Column<long>(type: "bigint", nullable: true),
                    DigitalSet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Step = table.Column<bool>(type: "bit", nullable: false),
                    TimeCalculation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorCalculation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueCalculation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Creator = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
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
