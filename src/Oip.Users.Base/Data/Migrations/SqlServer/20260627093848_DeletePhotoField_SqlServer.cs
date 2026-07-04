using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Users.Base.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class DeletePhotoField_SqlServer : Migration
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
                type: "varbinary(max)",
                nullable: true,
                comment: "User photo");
        }
    }
}
