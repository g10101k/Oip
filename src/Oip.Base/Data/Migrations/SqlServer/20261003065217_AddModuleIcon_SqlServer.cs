using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Base.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddModuleIcon_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                schema: "oip",
                table: "Module",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "Default icon for new module instances");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                schema: "oip",
                table: "Module");
        }
    }
}
