using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Oip.Notifications.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddChanelAndDates_Postgres : Migration
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
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "User identifier");

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                schema: "notifications",
                table: "NotificationUser",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
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
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Message of the notification for this user");

            migrationBuilder.AlterColumn<int>(
                name: "Importance",
                schema: "notifications",
                table: "NotificationUser",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
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
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeliveredAt",
                schema: "notifications",
                table: "NotificationUser",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NotificationChannelId",
                schema: "notifications",
                table: "NotificationUser",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReadAt",
                schema: "notifications",
                table: "NotificationUser",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SentAt",
                schema: "notifications",
                table: "NotificationUser",
                type: "timestamp with time zone",
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
                type: "integer",
                nullable: false,
                comment: "User identifier",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                schema: "notifications",
                table: "NotificationUser",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                comment: "Subject of the notification for this user",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
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
                type: "text",
                nullable: false,
                comment: "Message of the notification for this user",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Importance",
                schema: "notifications",
                table: "NotificationUser",
                type: "integer",
                nullable: false,
                comment: "Importance level of the notification",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "NotificationUserId",
                schema: "notifications",
                table: "NotificationUser",
                type: "bigint",
                nullable: false,
                comment: "Unique identifier for the notification user",
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
