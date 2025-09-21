﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Rtds.Data.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RenameFieldId23 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tag_InterfaceEntity_InterfaceId",
                schema: "rtds",
                table: "Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InterfaceEntity",
                table: "InterfaceEntity");

            migrationBuilder.RenameTable(
                name: "InterfaceEntity",
                newName: "Interface",
                newSchema: "rtds");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "rtds",
                table: "Interface",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Interface",
                schema: "rtds",
                table: "Interface",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Interface_Name",
                schema: "rtds",
                table: "Interface",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_Interface_InterfaceId",
                schema: "rtds",
                table: "Tag",
                column: "InterfaceId",
                principalSchema: "rtds",
                principalTable: "Interface",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tag_Interface_InterfaceId",
                schema: "rtds",
                table: "Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Interface",
                schema: "rtds",
                table: "Interface");

            migrationBuilder.DropIndex(
                name: "IX_Interface_Name",
                schema: "rtds",
                table: "Interface");

            migrationBuilder.RenameTable(
                name: "Interface",
                schema: "rtds",
                newName: "InterfaceEntity");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "InterfaceEntity",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AddPrimaryKey(
                name: "PK_InterfaceEntity",
                table: "InterfaceEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_InterfaceEntity_InterfaceId",
                schema: "rtds",
                table: "Tag",
                column: "InterfaceId",
                principalTable: "InterfaceEntity",
                principalColumn: "Id");
        }
    }
}
