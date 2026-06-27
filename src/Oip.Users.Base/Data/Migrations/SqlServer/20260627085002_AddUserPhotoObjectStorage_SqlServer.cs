using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Users.Base.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddUserPhotoObjectStorage_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Settings",
                schema: "usr",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                comment: "User settings in json",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldComment: "User settings");

            migrationBuilder.AddColumn<string>(
                name: "PhotoContentType",
                schema: "usr",
                table: "User",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                comment: "User photo content type.");

            migrationBuilder.AddColumn<string>(
                name: "PhotoObjectName",
                schema: "usr",
                table: "User",
                type: "nvarchar(512)",
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
                type: "nvarchar(max)",
                nullable: false,
                comment: "User settings",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldComment: "User settings in json");
        }
    }
}
