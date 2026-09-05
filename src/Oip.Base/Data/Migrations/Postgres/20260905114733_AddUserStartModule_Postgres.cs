using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Oip.Base.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddUserStartModule_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserStartModule",
                schema: "oip",
                columns: table => new
                {
                    UserStartModuleId = table.Column<int>(type: "integer", nullable: false, comment: "Id")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserSubject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Stable user identifier taken from the subject claim (max 255 chars)"),
                    ModuleInstanceId = table.Column<int>(type: "integer", nullable: false, comment: "Module instance opened when no explicit route is requested")
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
