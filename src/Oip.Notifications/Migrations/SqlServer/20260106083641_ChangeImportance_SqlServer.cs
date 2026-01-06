using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Notifications.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class ChangeImportance_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notification_Importance",
                schema: "notifications",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "Importance",
                schema: "notifications",
                table: "Notification");

            migrationBuilder.AddColumn<int>(
                name: "Importance",
                schema: "notifications",
                table: "NotificationUser",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Importance level of the notification");

            migrationBuilder.AddColumn<int>(
                name: "Importance",
                schema: "notifications",
                table: "NotificationTemplate",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Importance level of the notification");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Importance",
                schema: "notifications",
                table: "NotificationUser");

            migrationBuilder.DropColumn(
                name: "Importance",
                schema: "notifications",
                table: "NotificationTemplate");

            migrationBuilder.AddColumn<int>(
                name: "Importance",
                schema: "notifications",
                table: "Notification",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Importance level of the notification");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_Importance",
                schema: "notifications",
                table: "Notification",
                column: "Importance");
        }
    }
}
