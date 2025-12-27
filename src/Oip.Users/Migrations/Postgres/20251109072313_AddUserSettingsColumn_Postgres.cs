using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Users.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddUserSettingsColumn_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Settings",
                schema: "usr",
                table: "User",
                type: "text",
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
