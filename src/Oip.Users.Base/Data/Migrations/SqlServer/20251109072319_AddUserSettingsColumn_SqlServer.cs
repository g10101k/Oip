#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Oip.Users.Base.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddUserSettingsColumn_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Settings",
                schema: "usr",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                comment: "User settings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Settings",
                schema: "usr",
                table: "User");
        }
    }
}
