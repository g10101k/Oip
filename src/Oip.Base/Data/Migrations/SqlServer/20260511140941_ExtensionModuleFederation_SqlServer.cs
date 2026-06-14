using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class ExtensionModuleFederation_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComponentName",
                schema: "oip",
                table: "Module",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                comment: "Exported Angular component name.");

            migrationBuilder.AddColumn<string>(
                name: "ExposedModule",
                schema: "oip",
                table: "Module",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                comment: "Module Federation exposed module name.");

            migrationBuilder.AddColumn<string>(
                name: "LoadType",
                schema: "oip",
                table: "Module",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "Extension loader type.");

            migrationBuilder.AddColumn<string>(
                name: "RemoteEntryUrl",
                schema: "oip",
                table: "Module",
                type: "nvarchar(2048)",
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
