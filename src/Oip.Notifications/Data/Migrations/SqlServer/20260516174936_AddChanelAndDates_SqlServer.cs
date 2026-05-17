using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Notifications.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddChanelAndDates_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "NotificationUser",
                schema: "notifications",
                oldComment: "Users who should be notified about an event");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                schema: "notifications",
                table: "NotificationUser",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "User identifier");

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                schema: "notifications",
                table: "NotificationUser",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldComment: "Subject of the notification for this user");

            migrationBuilder.AlterColumn<long>(
                name: "NotificationId",
                schema: "notifications",
                table: "NotificationUser",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "Notification identifier");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                schema: "notifications",
                table: "NotificationUser",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldComment: "Message of the notification for this user");

            migrationBuilder.AlterColumn<int>(
                name: "Importance",
                schema: "notifications",
                table: "NotificationUser",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Importance level of the notification");

            migrationBuilder.AlterColumn<long>(
                name: "NotificationUserId",
                schema: "notifications",
                table: "NotificationUser",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "Unique identifier for the notification user")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeliveredAt",
                schema: "notifications",
                table: "NotificationUser",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NotificationChannelId",
                schema: "notifications",
                table: "NotificationUser",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReadAt",
                schema: "notifications",
                table: "NotificationUser",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SentAt",
                schema: "notifications",
                table: "NotificationUser",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationUser_DeliveredAt",
                schema: "notifications",
                table: "NotificationUser",
                column: "DeliveredAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationUser_NotificationChannelId",
                schema: "notifications",
                table: "NotificationUser",
                column: "NotificationChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationUser_ReadAt",
                schema: "notifications",
                table: "NotificationUser",
                column: "ReadAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationUser_SentAt",
                schema: "notifications",
                table: "NotificationUser",
                column: "SentAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NotificationUser_DeliveredAt",
                schema: "notifications",
                table: "NotificationUser");

            migrationBuilder.DropIndex(
                name: "IX_NotificationUser_NotificationChannelId",
                schema: "notifications",
                table: "NotificationUser");

            migrationBuilder.DropIndex(
                name: "IX_NotificationUser_ReadAt",
                schema: "notifications",
                table: "NotificationUser");

            migrationBuilder.DropIndex(
                name: "IX_NotificationUser_SentAt",
                schema: "notifications",
                table: "NotificationUser");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                schema: "notifications",
                table: "NotificationUser");

            migrationBuilder.DropColumn(
                name: "NotificationChannelId",
                schema: "notifications",
                table: "NotificationUser");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                schema: "notifications",
                table: "NotificationUser");

            migrationBuilder.DropColumn(
                name: "SentAt",
                schema: "notifications",
                table: "NotificationUser");

            migrationBuilder.AlterTable(
                name: "NotificationUser",
                schema: "notifications",
                comment: "Users who should be notified about an event");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                schema: "notifications",
                table: "NotificationUser",
                type: "int",
                nullable: false,
                comment: "User identifier",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                schema: "notifications",
                table: "NotificationUser",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                comment: "Subject of the notification for this user",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<long>(
                name: "NotificationId",
                schema: "notifications",
                table: "NotificationUser",
                type: "bigint",
                nullable: false,
                comment: "Notification identifier",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                schema: "notifications",
                table: "NotificationUser",
                type: "nvarchar(max)",
                nullable: false,
                comment: "Message of the notification for this user",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Importance",
                schema: "notifications",
                table: "NotificationUser",
                type: "int",
                nullable: false,
                comment: "Importance level of the notification",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "NotificationUserId",
                schema: "notifications",
                table: "NotificationUser",
                type: "bigint",
                nullable: false,
                comment: "Unique identifier for the notification user",
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");
        }
    }
}
