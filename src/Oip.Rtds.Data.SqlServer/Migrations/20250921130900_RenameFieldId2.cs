using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Rtds.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class RenameFieldId2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Interface",
                schema: "rtds",
                table: "Tag");

            migrationBuilder.AddColumn<long>(
                name: "InterfaceId",
                schema: "rtds",
                table: "Tag",
                type: "bigint",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InterfaceEntity",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterfaceEntity", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tag_InterfaceId",
                schema: "rtds",
                table: "Tag",
                column: "InterfaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_InterfaceEntity_InterfaceId",
                schema: "rtds",
                table: "Tag",
                column: "InterfaceId",
                principalTable: "InterfaceEntity",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tag_InterfaceEntity_InterfaceId",
                schema: "rtds",
                table: "Tag");

            migrationBuilder.DropTable(
                name: "InterfaceEntity");

            migrationBuilder.DropIndex(
                name: "IX_Tag_InterfaceId",
                schema: "rtds",
                table: "Tag");

            migrationBuilder.DropColumn(
                name: "InterfaceId",
                schema: "rtds",
                table: "Tag");

            migrationBuilder.AddColumn<long>(
                name: "Interface",
                schema: "rtds",
                table: "Tag",
                type: "bigint",
                maxLength: 128,
                nullable: false,
                defaultValue: 0L);
        }
    }
}
