#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Oip.Base.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "User",
                schema: "oip",
                comment: "User entity");

            migrationBuilder.AlterTable(
                name: "ModuleSecurity",
                schema: "oip",
                comment: "Module security entity");

            migrationBuilder.AlterTable(
                name: "ModuleInstanceSecurity",
                schema: "oip",
                comment: "Module instance security entity");

            migrationBuilder.AlterTable(
                name: "ModuleInstance",
                schema: "oip",
                comment: "Module instance");

            migrationBuilder.AlterTable(
                name: "Module",
                schema: "oip",
                comment: "Module entity");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                schema: "oip",
                table: "User",
                type: "integer",
                nullable: false,
                comment: "User id",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Photo",
                schema: "oip",
                table: "User",
                type: "bytea",
                nullable: true,
                comment: "",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "oip",
                table: "User",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                comment: "E-mail",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                schema: "oip",
                table: "ModuleSecurity",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                comment: "Role",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Right",
                schema: "oip",
                table: "ModuleSecurity",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                comment: "Right",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<int>(
                name: "ModuleSecurityId",
                schema: "oip",
                table: "ModuleSecurity",
                type: "integer",
                nullable: false,
                comment: "Id",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "ModuleId",
                schema: "oip",
                table: "ModuleSecurity",
                type: "integer",
                nullable: false,
                comment: "ModuleId",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                schema: "oip",
                table: "ModuleInstanceSecurity",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                comment: "Role (max 255 chars)",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Right",
                schema: "oip",
                table: "ModuleInstanceSecurity",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                comment: "Right (max 255 chars)",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<int>(
                name: "ModuleInstanceSecurityId",
                schema: "oip",
                table: "ModuleInstanceSecurity",
                type: "integer",
                nullable: false,
                comment: "Id",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "ModuleInstanceId",
                schema: "oip",
                table: "ModuleInstanceSecurity",
                type: "integer",
                nullable: false,
                comment: "ModuleId",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                schema: "oip",
                table: "ModuleInstance",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true,
                comment: "Url",
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Target",
                schema: "oip",
                table: "ModuleInstance",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                comment: "Target",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Settings",
                schema: "oip",
                table: "ModuleInstance",
                type: "text",
                nullable: false,
                comment: "Settings",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "ParentId",
                schema: "oip",
                table: "ModuleInstance",
                type: "integer",
                nullable: true,
                comment: "Parent id",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ModuleInstanceId",
                schema: "oip",
                table: "ModuleInstance",
                type: "integer",
                nullable: false,
                comment: "Id",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "ModuleId",
                schema: "oip",
                table: "ModuleInstance",
                type: "integer",
                nullable: false,
                comment: "Module id",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Label",
                schema: "oip",
                table: "ModuleInstance",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                comment: "Label",
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                schema: "oip",
                table: "ModuleInstance",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                comment: "Label",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Settings",
                schema: "oip",
                table: "Module",
                type: "text",
                nullable: true,
                comment: "Settings for module",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RouterLink",
                schema: "oip",
                table: "Module",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                comment: "Route link to component",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "oip",
                table: "Module",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                comment: "Name",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<int>(
                name: "ModuleId",
                schema: "oip",
                table: "Module",
                type: "integer",
                nullable: false,
                comment: "Id",
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
                schema: "oip",
                oldComment: "User entity");

            migrationBuilder.AlterTable(
                name: "ModuleSecurity",
                schema: "oip",
                oldComment: "Module security entity");

            migrationBuilder.AlterTable(
                name: "ModuleInstanceSecurity",
                schema: "oip",
                oldComment: "Module instance security entity");

            migrationBuilder.AlterTable(
                name: "ModuleInstance",
                schema: "oip",
                oldComment: "Module instance");

            migrationBuilder.AlterTable(
                name: "Module",
                schema: "oip",
                oldComment: "Module entity");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                schema: "oip",
                table: "User",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "User id")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Photo",
                schema: "oip",
                table: "User",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true,
                oldComment: "");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "oip",
                table: "User",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldComment: "E-mail");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                schema: "oip",
                table: "ModuleSecurity",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldComment: "Role");

            migrationBuilder.AlterColumn<string>(
                name: "Right",
                schema: "oip",
                table: "ModuleSecurity",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldComment: "Right");

            migrationBuilder.AlterColumn<int>(
                name: "ModuleSecurityId",
                schema: "oip",
                table: "ModuleSecurity",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Id")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "ModuleId",
                schema: "oip",
                table: "ModuleSecurity",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "ModuleId");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                schema: "oip",
                table: "ModuleInstanceSecurity",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldComment: "Role (max 255 chars)");

            migrationBuilder.AlterColumn<string>(
                name: "Right",
                schema: "oip",
                table: "ModuleInstanceSecurity",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldComment: "Right (max 255 chars)");

            migrationBuilder.AlterColumn<int>(
                name: "ModuleInstanceSecurityId",
                schema: "oip",
                table: "ModuleInstanceSecurity",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Id")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "ModuleInstanceId",
                schema: "oip",
                table: "ModuleInstanceSecurity",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "ModuleId");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                schema: "oip",
                table: "ModuleInstance",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024,
                oldNullable: true,
                oldComment: "Url");

            migrationBuilder.AlterColumn<string>(
                name: "Target",
                schema: "oip",
                table: "ModuleInstance",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "Target");

            migrationBuilder.AlterColumn<string>(
                name: "Settings",
                schema: "oip",
                table: "ModuleInstance",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Settings");

            migrationBuilder.AlterColumn<int>(
                name: "ParentId",
                schema: "oip",
                table: "ModuleInstance",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Parent id");

            migrationBuilder.AlterColumn<int>(
                name: "ModuleInstanceId",
                schema: "oip",
                table: "ModuleInstance",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Id")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "ModuleId",
                schema: "oip",
                table: "ModuleInstance",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Module id");

            migrationBuilder.AlterColumn<string>(
                name: "Label",
                schema: "oip",
                table: "ModuleInstance",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldComment: "Label");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                schema: "oip",
                table: "ModuleInstance",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "Label");

            migrationBuilder.AlterColumn<string>(
                name: "Settings",
                schema: "oip",
                table: "Module",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Settings for module");

            migrationBuilder.AlterColumn<string>(
                name: "RouterLink",
                schema: "oip",
                table: "Module",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true,
                oldComment: "Route link to component");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "oip",
                table: "Module",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldComment: "Name");

            migrationBuilder.AlterColumn<int>(
                name: "ModuleId",
                schema: "oip",
                table: "Module",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Id")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
