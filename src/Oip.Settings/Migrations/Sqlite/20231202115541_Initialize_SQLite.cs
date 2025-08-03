#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Oip.Settings.Migrations.Sqlite;

/// <inheritdoc />
public partial class Initialize_SQLite : SettingsMigration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: AppSettingsTable,
            columns: table => new
            {
                Key = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                Value = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey($"PK_{AppSettingsTable}", x => x.Key);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: AppSettingsTable);
    }
}