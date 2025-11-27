using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Oip.Users.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddCommentAndPrimaryKeyHook_Postgres : Migration
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
                type: "timestamp with time zone",
                nullable: false,
                comment: "Last update date and time",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Photo",
                schema: "usr",
                table: "User",
                type: "bytea",
                nullable: true,
                comment: "User photo",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                schema: "usr",
                table: "User",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Last synchronization date and time",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                schema: "usr",
                table: "User",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                comment: "Last name",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "KeycloakId",
                schema: "usr",
                table: "User",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                comment: "Gets or sets the Keycloak identifier for the user.",
                oldClrType: typeof(string),
                oldType: "character varying(36)",
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "usr",
                table: "User",
                type: "boolean",
                nullable: false,
                comment: "Indicates whether the user is active",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                schema: "usr",
                table: "User",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                comment: "First name",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "usr",
                table: "User",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                comment: "E-mail",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "usr",
                table: "User",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Creation date and time",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                schema: "usr",
                table: "User",
                type: "integer",
                nullable: false,
                comment: "User id",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
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
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "User id")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "usr",
                table: "User",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldComment: "Last update date and time");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Photo",
                schema: "usr",
                table: "User",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true,
                oldComment: "User photo");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LastSyncedAt",
                schema: "usr",
                table: "User",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldComment: "Last synchronization date and time");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                schema: "usr",
                table: "User",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldComment: "Last name");

            migrationBuilder.AlterColumn<string>(
                name: "KeycloakId",
                schema: "usr",
                table: "User",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(36)",
                oldMaxLength: 36,
                oldComment: "Gets or sets the Keycloak identifier for the user.");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "usr",
                table: "User",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "Indicates whether the user is active");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                schema: "usr",
                table: "User",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldComment: "First name");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "usr",
                table: "User",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldComment: "E-mail");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "usr",
                table: "User",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldComment: "Creation date and time");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                schema: "usr",
                table: "User",
                column: "UserId");
        }
    }
}
