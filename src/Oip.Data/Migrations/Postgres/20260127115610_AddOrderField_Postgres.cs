using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddOrderField_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                schema: "oip",
                table: "ModuleInstance",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Module instance order");

            migrationBuilder.Sql("""
                                     UPDATE oip."ModuleInstance" mi
                                     SET "Order" = subquery.position
                                     FROM (
                                         SELECT 
                                             mi_inner."ModuleInstanceId",
                                             ROW_NUMBER() OVER (PARTITION BY mi_inner."ParentId" ORDER BY mi_inner."ModuleInstanceId") - 1 as position
                                         FROM oip."ModuleInstance" mi_inner
                                     ) subquery
                                     WHERE mi."ModuleInstanceId" = subquery."ModuleInstanceId"
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