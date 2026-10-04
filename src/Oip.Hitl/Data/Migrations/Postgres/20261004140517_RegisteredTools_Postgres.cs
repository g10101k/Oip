using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Hitl.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class RegisteredTools_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegisteredTool",
                schema: "llm",
                columns: table => new
                {
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Name of the tool, the primary key."),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, comment: "What the tool does."),
                    ParametersSchema = table.Column<string>(type: "text", nullable: false, comment: "JSON schema of the arguments object."),
                    ActivityName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Temporal name of the activity."),
                    HasArguments = table.Column<bool>(type: "boolean", nullable: false, comment: "Whether the activity takes the arguments object as its parameter."),
                    TaskQueue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Task queue the worker of the tool polls."),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false, comment: "Timeout of an attempt in seconds."),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: false, comment: "Attempts before the failure is returned to the model."),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false, comment: "Whether the user allows each call before it is made."),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "When the worker registered the tool (UTC).")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredTool", x => x.Name);
                },
                comment: "Tool registered by a skill worker: an activity of the worker the model may call, on the task queue of the worker.\n            The worker replaces its tools when it starts.");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredTool_TaskQueue",
                schema: "llm",
                table: "RegisteredTool",
                column: "TaskQueue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegisteredTool",
                schema: "llm");
        }
    }
}
