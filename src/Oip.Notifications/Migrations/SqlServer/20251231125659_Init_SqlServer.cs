using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Notifications.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class Init_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "notifications");

            migrationBuilder.CreateTable(
                name: "DataProtectionKey",
                schema: "notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FriendlyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Xml = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationChannel",
                schema: "notifications",
                columns: table => new
                {
                    NotificationChannelId = table.Column<int>(type: "int", nullable: false, comment: "Unique identifier for the notification channel")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Name of the notification channel"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "Whether the channel is active"),
                    RequiresVerification = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Whether the channel requires user verification"),
                    MaxRetryCount = table.Column<int>(type: "int", nullable: true, comment: "Maximum number of delivery retry attempts")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationChannel", x => x.NotificationChannelId);
                },
                comment: "Notification delivery channels");

            migrationBuilder.CreateTable(
                name: "NotificationType",
                schema: "notifications",
                columns: table => new
                {
                    NotificationTypeId = table.Column<int>(type: "int", nullable: false, comment: "Unique identifier for the notification type")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Name of the notification type"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "Detailed explanation of the notification type"),
                    Scope = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Scope of notification type (global, appName, feature)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationType", x => x.NotificationTypeId);
                },
                comment: "Type of notification within the system");

            migrationBuilder.CreateTable(
                name: "Notification",
                schema: "notifications",
                columns: table => new
                {
                    NotificationId = table.Column<long>(type: "bigint", nullable: false, comment: "Unique identifier for the notification")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationTypeId = table.Column<int>(type: "int", nullable: false, comment: "Notification type identifier"),
                    Importance = table.Column<int>(type: "int", nullable: false, comment: "Importance level of the notification"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, comment: "Creation timestamp of the notification"),
                    DataJson = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "JSON data associated with the notification")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notification_NotificationType_NotificationTypeId",
                        column: x => x.NotificationTypeId,
                        principalSchema: "notifications",
                        principalTable: "NotificationType",
                        principalColumn: "NotificationTypeId",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Notification/event");

            migrationBuilder.CreateTable(
                name: "NotificationTemplate",
                schema: "notifications",
                columns: table => new
                {
                    NotificationTemplateId = table.Column<int>(type: "int", nullable: false, comment: "Unique identifier for the notification template")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationTypeId = table.Column<int>(type: "int", nullable: false, comment: "Unique identifier for the notification type"),
                    SubjectTemplate = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Subject template for the notification"),
                    MessageTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Message template for the notification"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "Whether the notification template is currently active"),
                    NotificationChannelEntityNotificationChannelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplate", x => x.NotificationTemplateId);
                    table.ForeignKey(
                        name: "FK_NotificationTemplate_NotificationChannel_NotificationChannelEntityNotificationChannelId",
                        column: x => x.NotificationChannelEntityNotificationChannelId,
                        principalSchema: "notifications",
                        principalTable: "NotificationChannel",
                        principalColumn: "NotificationChannelId");
                    table.ForeignKey(
                        name: "FK_NotificationTemplate_NotificationType_NotificationTypeId",
                        column: x => x.NotificationTypeId,
                        principalSchema: "notifications",
                        principalTable: "NotificationType",
                        principalColumn: "NotificationTypeId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Notification template for different channels");

            migrationBuilder.CreateTable(
                name: "UserNotificationPreference",
                schema: "notifications",
                columns: table => new
                {
                    UserNotificationPreferenceId = table.Column<int>(type: "int", nullable: false, comment: "Unique identifier for the user notification preference")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false, comment: "User identifier"),
                    NotificationTypeId = table.Column<int>(type: "int", nullable: false, comment: "Notification type identifier"),
                    NotificationChannelId = table.Column<int>(type: "int", nullable: false, comment: "Notification channel identifier"),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "Whether notifications are enabled for this preference")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationPreference", x => x.UserNotificationPreferenceId);
                    table.ForeignKey(
                        name: "FK_UserNotificationPreference_NotificationChannel_NotificationChannelId",
                        column: x => x.NotificationChannelId,
                        principalSchema: "notifications",
                        principalTable: "NotificationChannel",
                        principalColumn: "NotificationChannelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserNotificationPreference_NotificationType_NotificationTypeId",
                        column: x => x.NotificationTypeId,
                        principalSchema: "notifications",
                        principalTable: "NotificationType",
                        principalColumn: "NotificationTypeId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "User notification preferences");

            migrationBuilder.CreateTable(
                name: "NotificationUser",
                schema: "notifications",
                columns: table => new
                {
                    NotificationUserId = table.Column<long>(type: "bigint", nullable: false, comment: "Unique identifier for the notification user")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationId = table.Column<long>(type: "bigint", nullable: false, comment: "Notification identifier"),
                    UserId = table.Column<int>(type: "int", nullable: false, comment: "User identifier"),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Subject of the notification for this user"),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "Message of the notification for this user")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationUser", x => x.NotificationUserId);
                    table.ForeignKey(
                        name: "FK_NotificationUser_Notification_NotificationId",
                        column: x => x.NotificationId,
                        principalSchema: "notifications",
                        principalTable: "Notification",
                        principalColumn: "NotificationId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Users who should be notified about an event");

            migrationBuilder.CreateTable(
                name: "NotificationTemplateChannel",
                schema: "notifications",
                columns: table => new
                {
                    NotificationTemplateChannelId = table.Column<int>(type: "int", nullable: false, comment: "Unique identifier for the template-channel association")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationTemplateId = table.Column<int>(type: "int", nullable: false, comment: "Notification template identifier"),
                    NotificationChannelId = table.Column<int>(type: "int", nullable: false, comment: "Notification channel identifier")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplateChannel", x => x.NotificationTemplateChannelId);
                    table.ForeignKey(
                        name: "FK_NotificationTemplateChannel_NotificationChannel_NotificationChannelId",
                        column: x => x.NotificationChannelId,
                        principalSchema: "notifications",
                        principalTable: "NotificationChannel",
                        principalColumn: "NotificationChannelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationTemplateChannel_NotificationTemplate_NotificationTemplateId",
                        column: x => x.NotificationTemplateId,
                        principalSchema: "notifications",
                        principalTable: "NotificationTemplate",
                        principalColumn: "NotificationTemplateId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Association between a notification template and a notification channel");

            migrationBuilder.CreateTable(
                name: "NotificationTemplateUser",
                schema: "notifications",
                columns: table => new
                {
                    NotificationTemplateUserId = table.Column<int>(type: "int", nullable: false, comment: "Unique identifier for the template-user mapping")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationTemplateId = table.Column<int>(type: "int", nullable: false, comment: "Notification template identifier"),
                    UserId = table.Column<int>(type: "int", nullable: false, comment: "User identifier")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplateUser", x => x.NotificationTemplateUserId);
                    table.ForeignKey(
                        name: "FK_NotificationTemplateUser_NotificationTemplate_NotificationTemplateId",
                        column: x => x.NotificationTemplateId,
                        principalSchema: "notifications",
                        principalTable: "NotificationTemplate",
                        principalColumn: "NotificationTemplateId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Mapping between a notification template and a user");

            migrationBuilder.CreateTable(
                name: "NotificationDelivery",
                schema: "notifications",
                columns: table => new
                {
                    NotificationDeliveryId = table.Column<long>(type: "bigint", nullable: false, comment: "Unique identifier for the notification delivery")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationUserId = table.Column<long>(type: "bigint", nullable: false, comment: "Notification user identifier"),
                    UserId = table.Column<int>(type: "int", nullable: false, comment: "User identifier"),
                    NotificationChannelId = table.Column<int>(type: "int", nullable: false, comment: "Notification channel identifier"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "Delivery status"),
                    ExternalId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, comment: "External identifier from the delivery service"),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, comment: "Error message if delivery failed"),
                    RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "Number of delivery retry attempts"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, comment: "Creation timestamp of the delivery record"),
                    SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "Timestamp when the notification was sent"),
                    DeliveredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "Timestamp when the notification was delivered")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationDelivery", x => x.NotificationDeliveryId);
                    table.ForeignKey(
                        name: "FK_NotificationDelivery_NotificationChannel_NotificationChannelId",
                        column: x => x.NotificationChannelId,
                        principalSchema: "notifications",
                        principalTable: "NotificationChannel",
                        principalColumn: "NotificationChannelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotificationDelivery_NotificationUser_NotificationUserId",
                        column: x => x.NotificationUserId,
                        principalSchema: "notifications",
                        principalTable: "NotificationUser",
                        principalColumn: "NotificationUserId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Notification delivery history by channels");

            migrationBuilder.InsertData(
                schema: "notifications",
                table: "DataProtectionKey",
                columns: new[] { "Id", "FriendlyName", "Xml" },
                values: new object[] { 1, "key-b57db670-000b-4dd7-8603-e82a29941b6d", "<key id=\"b57db670-000b-4dd7-8603-e82a29941b6d\" version=\"1\"><creationDate>2025-12-31T12:29:13.551409Z</creationDate><activationDate>2025-12-31T12:29:13.395011Z</activationDate><expirationDate>2125-12-07T12:29:13.395011Z</expirationDate><descriptor deserializerType=\"Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel.AuthenticatedEncryptorDescriptorDeserializer, Microsoft.AspNetCore.DataProtection, Version=8.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60\"><descriptor><encryption algorithm=\"AES_256_CBC\" /><validation algorithm=\"HMACSHA256\" /><masterKey p4:requiresEncryption=\"true\" xmlns:p4=\"http://schemas.asp.net/2015/03/dataProtection\"><!-- Warning: the key below is in an unencrypted form. --><value>y6SO3S3B9NggxGQguIPpg3kCnMYSJt9bed1u8VFDIOqmL4eyzvBZLuLHJY74ASdSAlNW5ayiRDWnWhEzc95IcA==</value></masterKey></descriptor></descriptor></key>" });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_CreatedAt",
                schema: "notifications",
                table: "Notification",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_Importance",
                schema: "notifications",
                table: "Notification",
                column: "Importance");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_NotificationTypeId",
                schema: "notifications",
                table: "Notification",
                column: "NotificationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationChannel_Name",
                schema: "notifications",
                table: "NotificationChannel",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDelivery_CreatedAt",
                schema: "notifications",
                table: "NotificationDelivery",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDelivery_ExternalId",
                schema: "notifications",
                table: "NotificationDelivery",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDelivery_NotificationChannelId",
                schema: "notifications",
                table: "NotificationDelivery",
                column: "NotificationChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDelivery_NotificationUserId",
                schema: "notifications",
                table: "NotificationDelivery",
                column: "NotificationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDelivery_SentAt",
                schema: "notifications",
                table: "NotificationDelivery",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDelivery_Status",
                schema: "notifications",
                table: "NotificationDelivery",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDelivery_Status_RetryCount",
                schema: "notifications",
                table: "NotificationDelivery",
                columns: new[] { "Status", "RetryCount" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDelivery_UserId",
                schema: "notifications",
                table: "NotificationDelivery",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_NotificationChannelEntityNotificationChannelId",
                schema: "notifications",
                table: "NotificationTemplate",
                column: "NotificationChannelEntityNotificationChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplate_NotificationTypeId_IsActive",
                schema: "notifications",
                table: "NotificationTemplate",
                columns: new[] { "NotificationTypeId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplateChannel_NotificationChannelId",
                schema: "notifications",
                table: "NotificationTemplateChannel",
                column: "NotificationChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplateChannel_NotificationTemplateId_NotificationChannelId",
                schema: "notifications",
                table: "NotificationTemplateChannel",
                columns: new[] { "NotificationTemplateId", "NotificationChannelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplateUser_NotificationTemplateId_UserId",
                schema: "notifications",
                table: "NotificationTemplateUser",
                columns: new[] { "NotificationTemplateId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplateUser_UserId",
                schema: "notifications",
                table: "NotificationTemplateUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationType_Name",
                schema: "notifications",
                table: "NotificationType",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationType_Scope",
                schema: "notifications",
                table: "NotificationType",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationUser_NotificationId",
                schema: "notifications",
                table: "NotificationUser",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationUser_NotificationId_UserId",
                schema: "notifications",
                table: "NotificationUser",
                columns: new[] { "NotificationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationUser_UserId",
                schema: "notifications",
                table: "NotificationUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreference_NotificationChannelId",
                schema: "notifications",
                table: "UserNotificationPreference",
                column: "NotificationChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreference_NotificationTypeId",
                schema: "notifications",
                table: "UserNotificationPreference",
                column: "NotificationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreference_UserId",
                schema: "notifications",
                table: "UserNotificationPreference",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreference_UserId_IsEnabled",
                schema: "notifications",
                table: "UserNotificationPreference",
                columns: new[] { "UserId", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreference_UserId_NotificationTypeId_NotificationChannelId",
                schema: "notifications",
                table: "UserNotificationPreference",
                columns: new[] { "UserId", "NotificationTypeId", "NotificationChannelId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataProtectionKey",
                schema: "notifications");

            migrationBuilder.DropTable(
                name: "NotificationDelivery",
                schema: "notifications");

            migrationBuilder.DropTable(
                name: "NotificationTemplateChannel",
                schema: "notifications");

            migrationBuilder.DropTable(
                name: "NotificationTemplateUser",
                schema: "notifications");

            migrationBuilder.DropTable(
                name: "UserNotificationPreference",
                schema: "notifications");

            migrationBuilder.DropTable(
                name: "NotificationUser",
                schema: "notifications");

            migrationBuilder.DropTable(
                name: "NotificationTemplate",
                schema: "notifications");

            migrationBuilder.DropTable(
                name: "Notification",
                schema: "notifications");

            migrationBuilder.DropTable(
                name: "NotificationChannel",
                schema: "notifications");

            migrationBuilder.DropTable(
                name: "NotificationType",
                schema: "notifications");
        }
    }
}
