using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Users.Base.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddUserPhotoObjectStorage_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Settings",
                schema: "usr",
                table: "User",
                type: "text",
                nullable: false,
                comment: "User settings in json",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "User settings");

            migrationBuilder.AddColumn<string>(
                name: "PhotoContentType",
                schema: "usr",
                table: "User",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                comment: "User photo content type.");

            migrationBuilder.AddColumn<string>(
                name: "PhotoObjectName",
                schema: "usr",
                table: "User",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true,
                comment: "User photo object name in object storage.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoContentType",
                schema: "usr",
                table: "User");

            migrationBuilder.DropColumn(
                name: "PhotoObjectName",
                schema: "usr",
                table: "User");

            migrationBuilder.AlterColumn<string>(
                name: "Settings",
                schema: "usr",
                table: "User",
                type: "text",
                nullable: false,
                comment: "User settings",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "User settings in json");
        }
    }
}
