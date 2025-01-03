using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class ChangeFeatureInstancePostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                schema: "oip",
                table: "FeatureInstance",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                schema: "oip",
                table: "FeatureInstance",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RouterLink",
                schema: "oip",
                table: "FeatureInstance",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Target",
                schema: "oip",
                table: "FeatureInstance",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                schema: "oip",
                table: "FeatureInstance",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                schema: "oip",
                table: "FeatureInstance");

            migrationBuilder.DropColumn(
                name: "Label",
                schema: "oip",
                table: "FeatureInstance");

            migrationBuilder.DropColumn(
                name: "RouterLink",
                schema: "oip",
                table: "FeatureInstance");

            migrationBuilder.DropColumn(
                name: "Target",
                schema: "oip",
                table: "FeatureInstance");

            migrationBuilder.DropColumn(
                name: "Url",
                schema: "oip",
                table: "FeatureInstance");
        }
    }
}
