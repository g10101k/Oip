#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Oip.Security.Dal.Sqlite.Migrations.IdentityServerDataProtection;

// ReSharper disable once UnusedType.Global
public partial class Init : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "DataProtectionKeys",
            table => new
            {
                Id = table.Column<int>("INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                FriendlyName = table.Column<string>("TEXT", nullable: true),
                Xml = table.Column<string>("TEXT", nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_DataProtectionKeys", x => x.Id); });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "DataProtectionKeys");
    }
}