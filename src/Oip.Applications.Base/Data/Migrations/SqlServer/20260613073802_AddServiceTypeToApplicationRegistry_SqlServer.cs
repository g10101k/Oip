using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Applications.Base.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddServiceTypeToApplicationRegistry_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceType",
                schema: "applications",
                table: "ApplicationRegistryItem",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Type of service.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceType",
                schema: "applications",
                table: "ApplicationRegistryItem");
        }
    }
}
