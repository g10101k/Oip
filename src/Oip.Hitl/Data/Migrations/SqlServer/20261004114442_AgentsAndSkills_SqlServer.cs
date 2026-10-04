using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Hitl.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AgentsAndSkills_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agent",
                schema: "llm",
                columns: table => new
                {
                    AgentId = table.Column<int>(type: "int", nullable: false, comment: "Primary key.")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Model id of the agent in the OpenAI-compatible API, e.g. support-agent."),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Human-readable name shown in the UI."),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, comment: "Description for administrators."),
                    SystemPrompt = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "System prompt of the agent; the list of its skills is added to it."),
                    LlmProviderId = table.Column<int>(type: "int", nullable: true, comment: "Provider that answers; null for the default provider."),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, comment: "Whether the agent is offered to chat UIs."),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Creation timestamp (UTC)."),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Last update timestamp (UTC).")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agent", x => x.AgentId);
                    table.ForeignKey(
                        name: "FK_Agent_LlmProvider_LlmProviderId",
                        column: x => x.LlmProviderId,
                        principalSchema: "llm",
                        principalTable: "LlmProvider",
                        principalColumn: "LlmProviderId",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "Agent exposed to chat UIs as a model: a system prompt, the LLM provider that answers and the skills it may load.");

            migrationBuilder.CreateTable(
                name: "Skill",
                schema: "llm",
                columns: table => new
                {
                    SkillId = table.Column<int>(type: "int", nullable: false, comment: "Primary key.")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Name the model loads the skill by, e.g. datetime."),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false, comment: "When to use the skill; always shown to the model."),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: false, comment: "How to do the task; given to the model when it loads the skill."),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, comment: "Whether agents may load the skill."),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Creation timestamp (UTC)."),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Last update timestamp (UTC).")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skill", x => x.SkillId);
                },
                comment: "Skill of agents: instructions and tools for a kind of task. The model sees only  until\n            it loads the skill; then it gets  and the tools.");

            migrationBuilder.CreateTable(
                name: "AgentSkill",
                schema: "llm",
                columns: table => new
                {
                    AgentId = table.Column<int>(type: "int", nullable: false, comment: "Agent."),
                    SkillId = table.Column<int>(type: "int", nullable: false, comment: "Skill.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentSkill", x => new { x.AgentId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_AgentSkill_Agent_AgentId",
                        column: x => x.AgentId,
                        principalSchema: "llm",
                        principalTable: "Agent",
                        principalColumn: "AgentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgentSkill_Skill_SkillId",
                        column: x => x.SkillId,
                        principalSchema: "llm",
                        principalTable: "Skill",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Skill an agent may load.");

            migrationBuilder.CreateTable(
                name: "SkillTool",
                schema: "llm",
                columns: table => new
                {
                    SkillId = table.Column<int>(type: "int", nullable: false, comment: "Skill."),
                    ToolName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Name of the tool, e.g. get_current_time.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillTool", x => new { x.SkillId, x.ToolName });
                    table.ForeignKey(
                        name: "FK_SkillTool_Skill_SkillId",
                        column: x => x.SkillId,
                        principalSchema: "llm",
                        principalTable: "Skill",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Tool a skill gives the model, by its name in the tool catalog.");

            migrationBuilder.CreateIndex(
                name: "IX_Agent_Code",
                schema: "llm",
                table: "Agent",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Agent_LlmProviderId",
                schema: "llm",
                table: "Agent",
                column: "LlmProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentSkill_SkillId",
                schema: "llm",
                table: "AgentSkill",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_Skill_Code",
                schema: "llm",
                table: "Skill",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentSkill",
                schema: "llm");

            migrationBuilder.DropTable(
                name: "SkillTool",
                schema: "llm");

            migrationBuilder.DropTable(
                name: "Agent",
                schema: "llm");

            migrationBuilder.DropTable(
                name: "Skill",
                schema: "llm");
        }
    }
}
