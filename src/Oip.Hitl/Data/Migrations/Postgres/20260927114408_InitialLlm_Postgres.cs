#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Oip.Hitl.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class InitialLlm_Postgres : Migration
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
                    LlmProviderId = table.Column<int>(type: "integer", nullable: false, comment: "Primary key.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Human-readable name shown in the UI."),
                    ProviderType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Provider API kind."),
                    BaseUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "API base URL, e.g. https://api.openai.com/v1."),
                    Model = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Default model identifier, e.g. gpt-4o-mini."),
                    ApiKeyProtected = table.Column<string>(type: "text", nullable: true, comment: "API key protected with ASP.NET Core Data Protection. Null when no key is configured."),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, comment: "Whether this provider is used when no provider is specified explicitly. At most one provider is default."),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, comment: "Whether the provider can be used."),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Creation timestamp (UTC)."),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Last update timestamp (UTC).")
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
                filter: "\"IsDefault\" = true");

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
