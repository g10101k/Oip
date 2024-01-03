#nullable disable

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Oip.Security.Dal.Sqlite.Migrations.AdminLog;

// ReSharper disable once UnusedType.Global
public partial class Init : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "Log",
            table => new
            {
                Id = table.Column<long>("INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Message = table.Column<string>("TEXT", nullable: true),
                MessageTemplate = table.Column<string>("TEXT", nullable: true),
                Level = table.Column<string>("TEXT", maxLength: 128, nullable: true),
                TimeStamp = table.Column<DateTimeOffset>("TEXT", nullable: false),
                Exception = table.Column<string>("TEXT", nullable: true),
                LogEvent = table.Column<string>("TEXT", nullable: true),
                Properties = table.Column<string>("TEXT", nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_Log", x => x.Id); });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "Log");
    }
}