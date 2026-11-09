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
                    CustomizationId = table.Column<int>(type: "integer", nullable: false,
                        comment: "Primary key of the customization record."),
                    Css = table.Column<string>(type: "text", nullable: false,
                        comment: "Contents of the custom stylesheet loaded by the OIP shell."),
                    JavaScript = table.Column<string>(type: "text", nullable: false,
                        comment: "Contents of the custom JavaScript file loaded by the OIP shell."),
                    Favicon = table.Column<byte[]>(type: "bytea", nullable: true,
                        comment: "Custom favicon bytes. A null value selects the default favicon."),
                    Logo = table.Column<byte[]>(type: "bytea", nullable: true,
                        comment: "Custom logo bytes. A null value selects the original logo.")
                },
                constraints: table => { table.PrimaryKey("PK_Customization", x => x.CustomizationId); },
                comment: "Stores the site-wide assets loaded by the OIP shell.");

            migrationBuilder.InsertData(
                schema: "oip",
                table: "Customization",
                columns: new[] { "CustomizationId", "Css", "JavaScript", "Favicon", "Logo" },
                values: new object[]
                {
                    1,
                    """
                    /*
                    html:root {
                      --p-primary-50: #f0fdfa;
                      --p-primary-100: #ccfbf1;
                      --p-primary-200: #99f6e4;
                      --p-primary-300: #5eead4;
                      --p-primary-400: #2dd4bf;
                      --p-primary-500: #14b8a6;
                      --p-primary-600: #0d9488;
                      --p-primary-700: #0f766e;
                      --p-primary-800: #115e59;
                      --p-primary-900: #134e4a;
                      --p-primary-950: #042f2e;

                      --p-surface-0: #ffffff;
                      --p-surface-50: #fafaf9;
                      --p-surface-100: #f5f5f4;
                      --p-surface-200: #e7e5e4;
                      --p-surface-300: #d6d3d1;
                      --p-surface-400: #a8a29e;
                      --p-surface-500: #78716c;
                      --p-surface-600: #57534e;
                      --p-surface-700: #44403c;
                      --p-surface-800: #292524;
                      --p-surface-900: #1c1917;
                      --p-surface-950: #0c0a09;

                      --p-primary-color: var(--p-primary-700);
                      --p-primary-hover-color: var(--p-primary-800);
                      --p-primary-active-color: var(--p-primary-900);
                      --p-primary-contrast-color: var(--p-surface-0);

                      --surface-ground: var(--p-surface-100);
                      --p-content-background: var(--p-surface-0);
                    }

                    html:root.app-dark {
                      --p-primary-color: var(--p-primary-300);
                      --p-primary-hover-color: var(--p-primary-200);
                      --p-primary-active-color: var(--p-primary-100);
                      --p-primary-contrast-color: var(--p-primary-950);

                      --surface-ground: var(--p-surface-900);
                      --p-content-background: var(--p-surface-800);
                    }

                    :root.app-dark .layout-sidebar {
                      background-color: var(--surface-card);
                    }

                    .p-button {
                      --p-button-border-radius: 0.75rem;
                    }

                    logo.logo-header {
                      --logo-max-width: 144px;
                      --logo-max-height: 18px;
                    }

                    logo.logo-footer {
                      --logo-max-width: 72px;
                      --logo-max-height: 18px;
                    }

                    logo.logo-error-page {
                      --logo-max-width: 96px;
                      --logo-max-height: 18px;
                    }
                    */
                    """,
                    string.Empty,
                    System.Text.Encoding.UTF8.GetBytes(InitialDefaultLogoSvg.Markup),
                    System.Text.Encoding.UTF8.GetBytes(InitialDefaultLogoSvg.Markup)
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