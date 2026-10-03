using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Base.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class FolderOnlyMenuRoot_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Settings",
                schema: "oip",
                table: "ModuleInstance",
                type: "text",
                nullable: true,
                comment: "Settings",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Settings");

            // The Folder module is registered on application startup, which runs after migrations,
            // so it is created here when missing to convert the menu exactly once.
            migrationBuilder.Sql("""
                INSERT INTO oip."Module" ("Name", "RouterLink", "Icon", "Kind")
                SELECT 'FolderModule', '/folder-module', 'pi pi-folder', 0
                WHERE NOT EXISTS (SELECT 1 FROM oip."Module" WHERE "Name" = 'FolderModule');
                """);

            // Only a folder can be placed at the menu root or have child items.
            migrationBuilder.Sql("""
                UPDATE oip."ModuleInstance" AS mi
                SET "ModuleId" = f."ModuleId", "Settings" = NULL
                FROM (SELECT "ModuleId" FROM oip."Module" WHERE "Name" = 'FolderModule'
                      ORDER BY "ModuleId" LIMIT 1) AS f
                WHERE mi."ModuleId" <> f."ModuleId"
                  AND (mi."ParentId" IS NULL
                       OR EXISTS (SELECT 1 FROM oip."ModuleInstance" AS c
                                  WHERE c."ParentId" = mi."ModuleInstanceId"));
                """);

            // A folder cannot be a start module.
            migrationBuilder.Sql("""
                DELETE FROM oip."UserStartModule" AS s
                USING oip."ModuleInstance" AS mi, oip."Module" AS m
                WHERE s."ModuleInstanceId" = mi."ModuleInstanceId"
                  AND mi."ModuleId" = m."ModuleId"
                  AND m."Name" = 'FolderModule';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Settings",
                schema: "oip",
                table: "ModuleInstance",
                type: "text",
                nullable: false,
                defaultValue: "",
                comment: "Settings",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Settings");
        }
    }
}
