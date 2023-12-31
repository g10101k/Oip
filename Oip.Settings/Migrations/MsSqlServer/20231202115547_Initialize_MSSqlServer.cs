using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Settings.Migrations.MsSqlServer
{
    /// <inheritdoc />
    public partial class Initialize_MSSqlServer : SettingsMigration
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
                    Key = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table => { table.PrimaryKey($"PK_{AppSettingsTable}", x => x.Key); });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: AppSettingsTable,
                schema: AppSettingsTable);
        }
    }
}