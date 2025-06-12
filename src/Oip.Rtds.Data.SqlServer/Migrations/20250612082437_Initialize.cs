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
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descriptor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EngUnits = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstrumentTag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Archiving = table.Column<bool>(type: "bit", nullable: true),
                    Compressing = table.Column<bool>(type: "bit", nullable: true),
                    ExcDev = table.Column<double>(type: "float", nullable: true),
                    ExcMin = table.Column<int>(type: "int", nullable: true),
                    ExcMax = table.Column<int>(type: "int", nullable: true),
                    CompDev = table.Column<double>(type: "float", nullable: true),
                    CompMin = table.Column<int>(type: "int", nullable: true),
                    CompMax = table.Column<int>(type: "int", nullable: true),
                    Zero = table.Column<int>(type: "int", nullable: true),
                    Span = table.Column<int>(type: "int", nullable: true),
                    Location1 = table.Column<int>(type: "int", nullable: true),
                    Location2 = table.Column<int>(type: "int", nullable: true),
                    Location3 = table.Column<int>(type: "int", nullable: true),
                    Location4 = table.Column<int>(type: "int", nullable: true),
                    Location5 = table.Column<int>(type: "int", nullable: true),
                    ExDesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Scan = table.Column<bool>(type: "bit", nullable: true),
                    DigitalSet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Step = table.Column<bool>(type: "bit", nullable: true),
                    Future = table.Column<bool>(type: "bit", nullable: true),
                    UserInt1 = table.Column<int>(type: "int", nullable: true),
                    UserInt2 = table.Column<int>(type: "int", nullable: true),
                    UserInt3 = table.Column<int>(type: "int", nullable: true),
                    UserInt4 = table.Column<int>(type: "int", nullable: true),
                    UserInt5 = table.Column<int>(type: "int", nullable: true),
                    UserReal1 = table.Column<double>(type: "float", nullable: true),
                    UserReal2 = table.Column<double>(type: "float", nullable: true),
                    UserReal3 = table.Column<double>(type: "float", nullable: true),
                    UserReal4 = table.Column<double>(type: "float", nullable: true),
                    UserReal5 = table.Column<double>(type: "float", nullable: true),
                    CreationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Creator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Partition = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
