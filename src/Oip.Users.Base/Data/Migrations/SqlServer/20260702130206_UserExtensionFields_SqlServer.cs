using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Users.Base.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class UserExtensionFields_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExtensionFieldMetadata",
                schema: "usr",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false, comment: "Metadata identifier.")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityCode = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Base entity code, for example User."),
                    TableSchema = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Extension table schema."),
                    TableName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Extension table name."),
                    FieldName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "API field name."),
                    DbColumn = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Physical database column name."),
                    Type = table.Column<int>(type: "int", nullable: false, comment: "Field value type."),
                    OptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "JSON-encoded list of extension field options."),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false, comment: "Whether a value is required."),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false, comment: "Whether the field is visible by default."),
                    IsSortable = table.Column<bool>(type: "bit", nullable: false, comment: "Whether the field can be sorted by the table API."),
                    IsFilterable = table.Column<bool>(type: "bit", nullable: false, comment: "Whether the field can be filtered by the table API."),
                    Order = table.Column<int>(type: "int", nullable: false, comment: "Default display order.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtensionFieldMetadata", x => x.Id);
                },
                comment: "Global metadata for a physical extension field.");

            migrationBuilder.CreateTable(
                name: "UserExtension",
                schema: "usr",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false, comment: "User primary key and extension row primary key.")
                },
                constraints: table =>
                {
                },
                comment: "Physical extension row for user-specific custom columns.");

            migrationBuilder.CreateIndex(
                name: "IX_ExtensionFieldMetadata_EntityCode_FieldName",
                schema: "usr",
                table: "ExtensionFieldMetadata",
                columns: new[] { "EntityCode", "FieldName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExtensionFieldMetadata_TableSchema_TableName_DbColumn",
                schema: "usr",
                table: "ExtensionFieldMetadata",
                columns: new[] { "TableSchema", "TableName", "DbColumn" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtensionFieldMetadata",
                schema: "usr");

            migrationBuilder.DropTable(
                name: "UserExtension",
                schema: "usr");
        }
    }
}
