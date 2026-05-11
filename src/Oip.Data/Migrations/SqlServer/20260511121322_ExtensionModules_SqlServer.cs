using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class ExtensionModules_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiBaseUrl",
                schema: "oip",
                table: "Module",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true,
                comment: "Backend API base URL for the extension service.");

            migrationBuilder.AddColumn<string>(
                name: "ElementName",
                schema: "oip",
                table: "Module",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                comment: "Custom element tag name exposed by the extension bundle.");

            migrationBuilder.AddColumn<string>(
                name: "ExtensionKey",
                schema: "oip",
                table: "Module",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                comment: "Stable extension key.");

            migrationBuilder.AddColumn<int>(
                name: "Kind",
                schema: "oip",
                table: "Module",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Module delivery kind.");

            migrationBuilder.AddColumn<string>(
                name: "ManifestUrl",
                schema: "oip",
                table: "Module",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true,
                comment: "URL of the extension manifest, if this module is an extension.");

            migrationBuilder.AddColumn<string>(
                name: "ScriptUrl",
                schema: "oip",
                table: "Module",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true,
                comment: "JavaScript entrypoint that registers the custom element.");

            migrationBuilder.AddColumn<string>(
                name: "Version",
                schema: "oip",
                table: "Module",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "Extension version.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiBaseUrl",
                schema: "oip",
                table: "Module");

            migrationBuilder.DropColumn(
                name: "ElementName",
                schema: "oip",
                table: "Module");

            migrationBuilder.DropColumn(
                name: "ExtensionKey",
                schema: "oip",
                table: "Module");

            migrationBuilder.DropColumn(
                name: "Kind",
                schema: "oip",
                table: "Module");

            migrationBuilder.DropColumn(
                name: "ManifestUrl",
                schema: "oip",
                table: "Module");

            migrationBuilder.DropColumn(
                name: "ScriptUrl",
                schema: "oip",
                table: "Module");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "oip",
                table: "Module");
        }
    }
}
