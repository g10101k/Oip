#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Oip.Hitl.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class InitialLlm_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "llm");

            migrationBuilder.CreateTable(
                name: "LlmProvider",
                schema: "llm",
                columns: table => new
                {
                    LlmProviderId = table.Column<int>(type: "int", nullable: false, comment: "Primary key.")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Human-readable name shown in the UI."),
                    ProviderType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Provider API kind."),
                    BaseUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "API base URL, e.g. https://api.openai.com/v1."),
                    Model = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Default model identifier, e.g. gpt-4o-mini."),
                    ApiKeyProtected = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "API key protected with ASP.NET Core Data Protection. Null when no key is configured."),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, comment: "Whether this provider is used when no provider is specified explicitly. At most one provider is default."),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, comment: "Whether the provider can be used."),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Creation timestamp (UTC)."),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Last update timestamp (UTC).")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LlmProvider", x => x.LlmProviderId);
                },
                comment: "Stored LLM provider configuration.");

            migrationBuilder.CreateIndex(
                name: "IX_LlmProvider_IsDefault",
                schema: "llm",
                table: "LlmProvider",
                column: "IsDefault",
                unique: true,
                filter: "[IsDefault] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_LlmProvider_Name",
                schema: "llm",
                table: "LlmProvider",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LlmProvider",
                schema: "llm");
        }
    }
}
