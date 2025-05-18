using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Base.Data.SqlServer.Migrations
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
                name: "Module",
                schema: "oip",
                columns: table => new
                {
                    ModuleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RouterLink = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "ModuleInstance",
                schema: "oip",
                columns: table => new
                {
                    ModuleInstanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Target = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "ModuleInstanceSecurity",
                schema: "oip",
                columns: table => new
                {
                    ModuleInstanceSecurityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleInstanceId = table.Column<int>(type: "int", nullable: false),
                    Right = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "ModuleSecurity",
                schema: "oip",
                columns: table => new
                {
                    ModuleSecurityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    Right = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "oip",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Photo = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Module",
                schema: "oip");

            migrationBuilder.DropTable(
                name: "ModuleInstance",
                schema: "oip");

            migrationBuilder.DropTable(
                name: "ModuleInstanceSecurity",
                schema: "oip");

            migrationBuilder.DropTable(
                name: "ModuleSecurity",
                schema: "oip");

            migrationBuilder.DropTable(
                name: "User",
                schema: "oip");
        }
    }
}
