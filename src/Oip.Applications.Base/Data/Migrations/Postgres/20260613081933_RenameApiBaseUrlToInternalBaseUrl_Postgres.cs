using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Applications.Base.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class RenameApiBaseUrlToInternalBaseUrl_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApiBaseUrl",
                schema: "applications",
                table: "ApplicationRegistryItem",
                newName: "InternalBaseUrl");

            migrationBuilder.AlterColumn<string>(
                name: "InternalBaseUrl",
                schema: "applications",
                table: "ApplicationRegistryItem",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                comment: "Internal base URL.",
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048,
                oldComment: "Backend API URL.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InternalBaseUrl",
                schema: "applications",
                table: "ApplicationRegistryItem",
                newName: "ApiBaseUrl");

            migrationBuilder.AlterColumn<string>(
                name: "ApiBaseUrl",
                schema: "applications",
                table: "ApplicationRegistryItem",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                comment: "Backend API URL.",
                oldClrType: typeof(string),
                oldType: "character varying(2048)",
                oldMaxLength: 2048,
                oldComment: "Internal base URL.");
        }
    }
}
