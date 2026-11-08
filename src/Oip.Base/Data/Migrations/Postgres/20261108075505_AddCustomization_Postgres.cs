using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Base.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddCustomization_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customization",
                schema: "oip",
                columns: table => new
                {
                    CustomizationId = table.Column<int>(type: "integer", nullable: false, comment: "Primary key of the customization record."),
                    Css = table.Column<string>(type: "text", nullable: false, comment: "Contents of the custom stylesheet loaded by the OIP shell."),
                    JavaScript = table.Column<string>(type: "text", nullable: false, comment: "Contents of the custom JavaScript file loaded by the OIP shell."),
                    Favicon = table.Column<byte[]>(type: "bytea", nullable: true, comment: "Custom favicon bytes. A null value selects the default favicon.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customization", x => x.CustomizationId);
                },
                comment: "Stores the site-wide assets loaded by the OIP shell.");

            migrationBuilder.InsertData(
                schema: "oip",
                table: "Customization",
                columns: new[] { "CustomizationId", "Css", "JavaScript" },
                values: new object[]
                {
                    1,
                    """
                    /*
                    :root .layout-topbar {
                      background-color: #eff6ff;
                    }

                    .app-dark .layout-topbar {
                      background-color: #172554;
                    }

                    .p-button {
                      --p-button-border-radius: 0.75rem;
                    }

                    html:root {
                      --p-primary-color: #1d4ed8;
                      --p-primary-hover-color: #1e40af;
                      --p-primary-active-color: #1e3a8a;
                      --p-primary-contrast-color: #ffffff;

                      --surface-ground: #f1f5f9;
                      --p-content-background: #ffffff;
                    }

                    html:root.app-dark {
                      --p-primary-color: #93c5fd;
                      --p-primary-hover-color: #bfdbfe;
                      --p-primary-active-color: #dbeafe;
                      --p-primary-contrast-color: #172554;

                      --surface-ground: #0f172a;
                      --p-content-background: #1e293b;
                    }
                    */
                    """,
                    string.Empty
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customization",
                schema: "oip");
        }
    }
}
