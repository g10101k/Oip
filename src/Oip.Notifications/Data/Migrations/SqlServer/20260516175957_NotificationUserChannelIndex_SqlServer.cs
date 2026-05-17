using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Notifications.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class NotificationUserChannelIndex_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NotificationUser_NotificationId_UserId",
                schema: "notifications",
                table: "NotificationUser");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationUser_NotificationId_UserId_NotificationChannelId",
                schema: "notifications",
                table: "NotificationUser",
                columns: new[] { "NotificationId", "UserId", "NotificationChannelId" },
                unique: true,
                filter: "[NotificationChannelId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NotificationUser_NotificationId_UserId_NotificationChannelId",
                schema: "notifications",
                table: "NotificationUser");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationUser_NotificationId_UserId",
                schema: "notifications",
                table: "NotificationUser",
                columns: new[] { "NotificationId", "UserId" },
                unique: true);
        }
    }
}
