#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Oip.Base.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class ExtensionModuleFederation_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComponentName",
                schema: "oip",
                table: "Module",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                comment: "Exported Angular component name.");

            migrationBuilder.AddColumn<string>(
                name: "ExposedModule",
                schema: "oip",
                table: "Module",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                comment: "Module Federation exposed module name.");

            migrationBuilder.AddColumn<string>(
                name: "LoadType",
                schema: "oip",
                table: "Module",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                comment: "Extension loader type.");

            migrationBuilder.AddColumn<string>(
                name: "RemoteEntryUrl",
                schema: "oip",
                table: "Module",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true,
                comment: "Module Federation remote entry URL.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComponentName",
                schema: "oip",
                table: "Module");

            migrationBuilder.DropColumn(
                name: "ExposedModule",
                schema: "oip",
                table: "Module");

            migrationBuilder.DropColumn(
                name: "LoadType",
                schema: "oip",
                table: "Module");

            migrationBuilder.DropColumn(
                name: "RemoteEntryUrl",
                schema: "oip",
                table: "Module");
        }
    }
}
