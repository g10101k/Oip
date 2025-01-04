using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class ChangeFeaturePostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RouteLink",
                schema: "oip",
                table: "Feature",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RouteLink",
                schema: "oip",
                table: "Feature");
        }
    }
}
