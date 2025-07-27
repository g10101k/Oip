#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Oip.Settings.Migrations.Postgres;

/// <inheritdoc />
public partial class Initialize_Postgres : SettingsMigration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: AppSettingsSchema);

        migrationBuilder.CreateTable(
            name: AppSettingsTable,
            schema: AppSettingsSchema,
            columns: table => new
            {
                Key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                Value = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table => { table.PrimaryKey($"PK_{AppSettingsTable}", x => x.Key); });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: AppSettingsTable,
            schema: AppSettingsSchema);
    }
}