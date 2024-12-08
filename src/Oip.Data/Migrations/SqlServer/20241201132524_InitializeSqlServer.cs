using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class InitializeSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "oip");

            migrationBuilder.CreateTable(
                name: "Feature",
                schema: "oip",
                columns: table => new
                {
                    FeatureId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "FeatureInstance",
                schema: "oip",
                columns: table => new
                {
                    FeatureInstanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeatureId = table.Column<int>(type: "int", nullable: false),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "FeatureInstanceSecurity",
                schema: "oip",
                columns: table => new
                {
                    FeatureInstanceSecurityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeatureInstanceId = table.Column<int>(type: "int", nullable: false),
                    Right = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "FeatureSecurity",
                schema: "oip",
                columns: table => new
                {
                    FeatureSecurityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeatureId = table.Column<int>(type: "int", nullable: false),
                    Right = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Feature",
                schema: "oip");

            migrationBuilder.DropTable(
                name: "FeatureInstance",
                schema: "oip");

            migrationBuilder.DropTable(
                name: "FeatureInstanceSecurity",
                schema: "oip");

            migrationBuilder.DropTable(
                name: "FeatureSecurity",
                schema: "oip");
        }
    }
}
