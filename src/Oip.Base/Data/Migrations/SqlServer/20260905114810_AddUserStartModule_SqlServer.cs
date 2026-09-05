using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Base.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddUserStartModule_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserStartModule",
                schema: "oip",
                columns: table => new
                {
                    UserStartModuleId = table.Column<int>(type: "int", nullable: false, comment: "Id")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserSubject = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "Stable user identifier taken from the subject claim (max 255 chars)"),
                    ModuleInstanceId = table.Column<int>(type: "int", nullable: false, comment: "Module instance opened when no explicit route is requested")
                },
                constraints: table =>
                {
                },
                comment: "Module instance a user opens by default");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserStartModule",
                schema: "oip");
        }
    }
}
