#nullable disable

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Oip.Security.Dal.Sqlite.Migrations.AdminAuditLog;

// ReSharper disable once UnusedType.Global
public partial class Init : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "AuditLog",
            table => new
            {
                Id = table.Column<long>("INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Event = table.Column<string>("TEXT", nullable: true),
                Source = table.Column<string>("TEXT", nullable: true),
                Category = table.Column<string>("TEXT", nullable: true),
                SubjectIdentifier = table.Column<string>("TEXT", nullable: true),
                SubjectName = table.Column<string>("TEXT", nullable: true),
                SubjectType = table.Column<string>("TEXT", nullable: true),
                SubjectAdditionalData = table.Column<string>("TEXT", nullable: true),
                Action = table.Column<string>("TEXT", nullable: true),
                Data = table.Column<string>("TEXT", nullable: true),
                Created = table.Column<DateTime>("TEXT", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_AuditLog", x => x.Id); });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "AuditLog");
    }
}