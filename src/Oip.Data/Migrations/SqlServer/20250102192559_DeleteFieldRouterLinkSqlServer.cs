using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class DeleteFieldRouterLinkSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RouterLink",
                schema: "oip",
                table: "FeatureInstance");

            migrationBuilder.DropColumn(
                name: "RouteLink",
                schema: "oip",
                table: "Feature");

            migrationBuilder.AddColumn<string>(
                name: "RouterLink",
                schema: "oip",
                table: "Feature",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RouterLink",
                schema: "oip",
                table: "Feature");

            migrationBuilder.AddColumn<string>(
                name: "RouterLink",
                schema: "oip",
                table: "FeatureInstance",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RouteLink",
                schema: "oip",
                table: "Feature",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
