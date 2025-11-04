using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Users.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddCommentAndPrimaryKeyHook_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                schema: "usr",
                table: "User");

            migrationBuilder.AlterTable(
                name: "User",
                schema: "usr",
                comment: "User entity");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "usr",
                table: "User",
                type: "datetimeoffset",
                nullable: false,
                comment: "Last update date and time",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Photo",
                schema: "usr",
                table: "User",
                type: "varbinary(max)",
                nullable: true,
                comment: "User photo",
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                schema: "usr",
                table: "User",
                type: "datetimeoffset",
                nullable: false,
                comment: "Last synchronization date and time",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                schema: "usr",
                table: "User",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                comment: "Last name",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "KeycloakId",
                schema: "usr",
                table: "User",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: false,
                comment: "Gets or sets the Keycloak identifier for the user.",
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "usr",
                table: "User",
                type: "bit",
                nullable: false,
                comment: "Indicates whether the user is active",
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                schema: "usr",
                table: "User",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                comment: "First name",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "usr",
                table: "User",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                comment: "E-mail",
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "usr",
                table: "User",
                type: "datetimeoffset",
                nullable: false,
                comment: "Creation date and time",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                schema: "usr",
                table: "User",
                type: "int",
                nullable: false,
                comment: "User id",
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "User",
                schema: "usr",
                oldComment: "User entity");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                schema: "usr",
                table: "User",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "User id")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "usr",
                table: "User",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldComment: "Last update date and time");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Photo",
                schema: "usr",
                table: "User",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true,
                oldComment: "User photo");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                schema: "usr",
                table: "User",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldComment: "Last synchronization date and time");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                schema: "usr",
                table: "User",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldComment: "Last name");

            migrationBuilder.AlterColumn<string>(
                name: "KeycloakId",
                schema: "usr",
                table: "User",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(36)",
                oldMaxLength: 36,
                oldComment: "Gets or sets the Keycloak identifier for the user.");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "usr",
                table: "User",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComment: "Indicates whether the user is active");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                schema: "usr",
                table: "User",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldComment: "First name");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "usr",
                table: "User",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512,
                oldComment: "E-mail");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "usr",
                table: "User",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldComment: "Creation date and time");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                schema: "usr",
                table: "User",
                column: "UserId");
        }
    }
}
