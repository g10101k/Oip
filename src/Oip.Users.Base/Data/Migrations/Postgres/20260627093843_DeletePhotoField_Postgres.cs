using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Users.Base.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class DeletePhotoField_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Photo",
                schema: "usr",
                table: "User");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Photo",
                schema: "usr",
                table: "User",
                type: "bytea",
                nullable: true,
                comment: "User photo");
        }
    }
}
