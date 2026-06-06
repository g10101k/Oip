using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Notifications.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class ChangeColumnSize_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                schema: "notifications",
                table: "NotificationType",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                comment: "Scope of notification type (global, appName, feature)",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldComment: "Scope of notification type (global, appName, feature)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "notifications",
                table: "NotificationType",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                comment: "Name of the notification type",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "Name of the notification type");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "notifications",
                table: "NotificationType",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true,
                comment: "Detailed explanation of the notification type",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "Detailed explanation of the notification type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Scope",
                schema: "notifications",
                table: "NotificationType",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                comment: "Scope of notification type (global, appName, feature)",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldComment: "Scope of notification type (global, appName, feature)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "notifications",
                table: "NotificationType",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                comment: "Name of the notification type",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldComment: "Name of the notification type");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "notifications",
                table: "NotificationType",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Detailed explanation of the notification type",
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024,
                oldNullable: true,
                oldComment: "Detailed explanation of the notification type");
        }
    }
}
