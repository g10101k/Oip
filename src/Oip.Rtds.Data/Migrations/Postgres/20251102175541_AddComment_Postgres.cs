using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Oip.Rtds.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddComment_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tag_Interface_InterfaceId",
                schema: "rtds",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_Tag_InterfaceId",
                schema: "rtds",
                table: "Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Interface",
                schema: "rtds",
                table: "Interface");

            migrationBuilder.AlterTable(
                name: "Tag",
                schema: "rtds",
                comment: "Represents the configuration and metadata of a tag.");

            migrationBuilder.AlterColumn<int>(
                name: "ValueType",
                schema: "rtds",
                table: "Tag",
                type: "integer",
                nullable: false,
                comment: "Data type of the point (e.g., Float32, Int32, Digital, String, Blob).",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "ValueCalculation",
                schema: "rtds",
                table: "Tag",
                type: "text",
                nullable: true,
                comment: "User-defined calculation or formula associated with the tag's value.",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Uom",
                schema: "rtds",
                table: "Tag",
                type: "text",
                nullable: true,
                comment: "Engineering units (e.g., °C, PSI, m³/h).",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TimeCalculation",
                schema: "rtds",
                table: "Tag",
                type: "text",
                nullable: true,
                comment: "Formula used to calculate the time associated with the tag's value.\n            Default `now()`;",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Step",
                schema: "rtds",
                table: "Tag",
                type: "boolean",
                nullable: false,
                comment: "Indicates whether values are treated as step (true) or interpolated (false).",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "rtds",
                table: "Tag",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                comment: "Name of the tag.",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<long>(
                name: "InterfaceId",
                schema: "rtds",
                table: "Tag",
                type: "bigint",
                maxLength: 128,
                nullable: true,
                comment: "The interface associated with the tag.",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InstrumentTag",
                schema: "rtds",
                table: "Tag",
                type: "text",
                nullable: true,
                comment: "Reference to the source signal or channel tag.",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ErrorCalculation",
                schema: "rtds",
                table: "Tag",
                type: "text",
                nullable: true,
                comment: "Formula used to calculate error values for the tag.",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                schema: "rtds",
                table: "Tag",
                type: "boolean",
                nullable: false,
                comment: "Indicates whether the point is archived.",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "DigitalSet",
                schema: "rtds",
                table: "Tag",
                type: "text",
                nullable: true,
                comment: "Associated digital state set name (for digital-type points).",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descriptor",
                schema: "rtds",
                table: "Tag",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true,
                comment: "Description of the point (used as a comment or label).",
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Creator",
                schema: "rtds",
                table: "Tag",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                comment: "User or process that created the tag.",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreationDate",
                schema: "rtds",
                table: "Tag",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Date and time when the tag was created.",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<long>(
                name: "CompressionMinTime",
                schema: "rtds",
                table: "Tag",
                type: "bigint",
                nullable: true,
                comment: "Minimum time (in milliseconds) between compressed values.\n            Values received within this period are discarded, regardless of their error margin.",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CompressionMaxTime",
                schema: "rtds",
                table: "Tag",
                type: "bigint",
                nullable: true,
                comment: "Maximum time (in milliseconds) between compressed values.",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Compressing",
                schema: "rtds",
                table: "Tag",
                type: "boolean",
                nullable: false,
                comment: "Indicates whether compression is enabled for this tag.",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "rtds",
                table: "Tag",
                type: "bigint",
                nullable: false,
                comment: "Unique identifier of the tag.",
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "Tag",
                schema: "rtds",
                oldComment: "Represents the configuration and metadata of a tag.");

            migrationBuilder.AlterColumn<int>(
                name: "ValueType",
                schema: "rtds",
                table: "Tag",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Data type of the point (e.g., Float32, Int32, Digital, String, Blob).");

            migrationBuilder.AlterColumn<string>(
                name: "ValueCalculation",
                schema: "rtds",
                table: "Tag",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "User-defined calculation or formula associated with the tag's value.");

            migrationBuilder.AlterColumn<string>(
                name: "Uom",
                schema: "rtds",
                table: "Tag",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Engineering units (e.g., °C, PSI, m³/h).");

            migrationBuilder.AlterColumn<string>(
                name: "TimeCalculation",
                schema: "rtds",
                table: "Tag",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Formula used to calculate the time associated with the tag's value.\n            Default `now()`;");

            migrationBuilder.AlterColumn<bool>(
                name: "Step",
                schema: "rtds",
                table: "Tag",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "Indicates whether values are treated as step (true) or interpolated (false).");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "rtds",
                table: "Tag",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512,
                oldComment: "Name of the tag.");

            migrationBuilder.AlterColumn<long>(
                name: "InterfaceId",
                schema: "rtds",
                table: "Tag",
                type: "bigint",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldMaxLength: 128,
                oldNullable: true,
                oldComment: "The interface associated with the tag.");

            migrationBuilder.AlterColumn<string>(
                name: "InstrumentTag",
                schema: "rtds",
                table: "Tag",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Reference to the source signal or channel tag.");

            migrationBuilder.AlterColumn<string>(
                name: "ErrorCalculation",
                schema: "rtds",
                table: "Tag",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Formula used to calculate error values for the tag.");

            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                schema: "rtds",
                table: "Tag",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "Indicates whether the point is archived.");

            migrationBuilder.AlterColumn<string>(
                name: "DigitalSet",
                schema: "rtds",
                table: "Tag",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Associated digital state set name (for digital-type points).");

            migrationBuilder.AlterColumn<string>(
                name: "Descriptor",
                schema: "rtds",
                table: "Tag",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024,
                oldNullable: true,
                oldComment: "Description of the point (used as a comment or label).");

            migrationBuilder.AlterColumn<string>(
                name: "Creator",
                schema: "rtds",
                table: "Tag",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldComment: "User or process that created the tag.");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreationDate",
                schema: "rtds",
                table: "Tag",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldComment: "Date and time when the tag was created.");

            migrationBuilder.AlterColumn<long>(
                name: "CompressionMinTime",
                schema: "rtds",
                table: "Tag",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComment: "Minimum time (in milliseconds) between compressed values.\n            Values received within this period are discarded, regardless of their error margin.");

            migrationBuilder.AlterColumn<long>(
                name: "CompressionMaxTime",
                schema: "rtds",
                table: "Tag",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComment: "Maximum time (in milliseconds) between compressed values.");

            migrationBuilder.AlterColumn<bool>(
                name: "Compressing",
                schema: "rtds",
                table: "Tag",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "Indicates whether compression is enabled for this tag.");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                schema: "rtds",
                table: "Tag",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "Unique identifier of the tag.")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Interface",
                schema: "rtds",
                table: "Interface",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_InterfaceId",
                schema: "rtds",
                table: "Tag",
                column: "InterfaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_Interface_InterfaceId",
                schema: "rtds",
                table: "Tag",
                column: "InterfaceId",
                principalSchema: "rtds",
                principalTable: "Interface",
                principalColumn: "Id");
        }
    }
}
