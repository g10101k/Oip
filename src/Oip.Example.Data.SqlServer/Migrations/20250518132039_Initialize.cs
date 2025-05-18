using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Example.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Initialize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "oip");

            migrationBuilder.CreateTable(
                name: "Example",
                schema: "oip",
                columns: table => new
                {
                    ExampleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Example", x => x.ExampleId);
                });

            migrationBuilder.InsertData(
                schema: "oip",
                table: "Example",
                columns: new[] { "ExampleId", "Name" },
                values: new object[] { 1, "name 1" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Example",
                schema: "oip");
        }
    }
}
