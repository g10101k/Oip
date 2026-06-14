#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Oip.Applications.Base.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class InitialApplicationRegistry_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "applications");

            migrationBuilder.CreateTable(
                name: "ApplicationRegistryItem",
                schema: "applications",
                columns: table => new
                {
                    ApplicationRegistryItemId = table.Column<long>(type: "bigint", nullable: false, comment: "Internal database identifier.")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Stable application code."),
                    DisplayName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false, comment: "Human-readable application name."),
                    BaseUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false, comment: "Frontend application URL."),
                    ApiBaseUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false, comment: "Backend API URL."),
                    Icon = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, comment: "PrimeIcons CSS class."),
                    Order = table.Column<int>(type: "int", nullable: false, comment: "Display order."),
                    Enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "Indicates whether application should be returned to frontend.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationRegistryItem", x => x.ApplicationRegistryItemId);
                },
                comment: "Persisted frontend application registry item.");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRegistryItem_Code",
                schema: "applications",
                table: "ApplicationRegistryItem",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationRegistryItem",
                schema: "applications");
        }
    }
}
