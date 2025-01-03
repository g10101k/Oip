using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class DeleteFieldRouterLinkPostgres : Migration
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
                type: "character varying(256)",
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
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RouteLink",
                schema: "oip",
                table: "Feature",
                type: "text",
                nullable: true);
        }
    }
}
