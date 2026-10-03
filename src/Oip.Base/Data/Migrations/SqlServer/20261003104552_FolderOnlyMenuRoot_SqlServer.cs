using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Base.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class FolderOnlyMenuRoot_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Settings",
                schema: "oip",
                table: "ModuleInstance",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Settings",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldComment: "Settings");

            // The Folder module is registered on application startup, which runs after migrations,
            // so it is created here when missing to convert the menu exactly once.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [oip].[Module] WHERE [Name] = N'FolderModule')
                    INSERT INTO [oip].[Module] ([Name], [RouterLink], [Icon], [Kind])
                    VALUES (N'FolderModule', N'/folder-module', N'pi pi-folder', 0);
                """);

            // Only a folder can be placed at the menu root or have child items.
            migrationBuilder.Sql("""
                UPDATE mi
                SET mi.[ModuleId] = f.[ModuleId], mi.[Settings] = NULL
                FROM [oip].[ModuleInstance] AS mi
                CROSS JOIN (SELECT TOP 1 [ModuleId] FROM [oip].[Module] WHERE [Name] = N'FolderModule'
                            ORDER BY [ModuleId]) AS f
                WHERE mi.[ModuleId] <> f.[ModuleId]
                  AND (mi.[ParentId] IS NULL
                       OR EXISTS (SELECT 1 FROM [oip].[ModuleInstance] AS c
                                  WHERE c.[ParentId] = mi.[ModuleInstanceId]));
                """);

            // A folder cannot be a start module.
            migrationBuilder.Sql("""
                DELETE s
                FROM [oip].[UserStartModule] AS s
                INNER JOIN [oip].[ModuleInstance] AS mi ON mi.[ModuleInstanceId] = s.[ModuleInstanceId]
                INNER JOIN [oip].[Module] AS m ON m.[ModuleId] = mi.[ModuleId]
                WHERE m.[Name] = N'FolderModule';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Settings",
                schema: "oip",
                table: "ModuleInstance",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                comment: "Settings",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Settings");
        }
    }
}
