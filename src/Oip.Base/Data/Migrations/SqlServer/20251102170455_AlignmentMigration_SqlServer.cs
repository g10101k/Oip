#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Oip.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AlignmentMigration_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Photo",
                schema: "oip",
                table: "User",
                type: "varbinary(max)",
                nullable: true,
                comment: "User photo",
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true,
                oldComment: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Photo",
                schema: "oip",
                table: "User",
                type: "varbinary(max)",
                nullable: true,
                comment: "",
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true,
                oldComment: "User photo");
        }
    }
}
