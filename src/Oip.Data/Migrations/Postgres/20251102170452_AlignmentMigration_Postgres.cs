#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Oip.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AlignmentMigration_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Photo",
                schema: "oip",
                table: "User",
                type: "bytea",
                nullable: true,
                comment: "User photo",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
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
                type: "bytea",
                nullable: true,
                comment: "",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true,
                oldComment: "User photo");
        }
    }
}
