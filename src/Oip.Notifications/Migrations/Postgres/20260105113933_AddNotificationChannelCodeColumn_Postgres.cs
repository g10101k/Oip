using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Notifications.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddNotificationChannelCodeColumn_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "notifications",
                table: "NotificationChannel",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "",
                comment: "Name of the notification channel");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationChannel_Code",
                schema: "notifications",
                table: "NotificationChannel",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NotificationChannel_Code",
                schema: "notifications",
                table: "NotificationChannel");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "notifications",
                table: "NotificationChannel");
        }
    }
}
