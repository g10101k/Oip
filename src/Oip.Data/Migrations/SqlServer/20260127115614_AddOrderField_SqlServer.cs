using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddOrderField_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                schema: "oip",
                table: "ModuleInstance",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Module position");

            migrationBuilder.Sql("""
                                     WITH OrderRanking AS (
                                         SELECT 
                                             ModuleInstanceId,
                                             ROW_NUMBER() OVER (PARTITION BY ParentId ORDER BY ModuleInstanceId) - 1 AS [Order]
                                         FROM oip.ModuleInstance
                                     )
                                     UPDATE mi
                                     SET [Order] = pr.[Order]
                                     FROM oip.ModuleInstance mi
                                     INNER JOIN OrderRanking pr ON mi.ModuleInstanceId = pr.ModuleInstanceId
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                schema: "oip",
                table: "ModuleInstance");
        }
    }
}