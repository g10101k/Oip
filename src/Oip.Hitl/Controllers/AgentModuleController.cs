using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Repositories;
using Oip.Base.Exceptions;
using Oip.Base.Security;
using Oip.Hitl.Controllers.Api;
using Oip.Hitl.Services;

namespace Oip.Hitl.Controllers;

/// <summary>
/// Module that manages agents, offered to chat UIs as models, and their skills.
/// </summary>
[ApiController]
[Authorize]
[Route("api/agent-module")]
public class AgentModuleController(AgentService agentService, ModuleRepository moduleRepository)
    : BaseModuleController<AgentModuleSettings>(moduleRepository)
{
    /// <inheritdoc />
    public override string? Icon => "pi pi-sparkles";

    /// <summary>
    /// Returns all agents.
    /// </summary>
    [Right(SecurityConstants.Read), HttpGet("get-agents")]
    [ProducesResponseType<List<AgentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AgentDto>>> GetAgents(CancellationToken cancellationToken)
    {
        return Ok(await agentService.GetAgentsAsync(cancellationToken));
    }

    /// <summary>
    /// Creates an agent.
    /// </summary>
    [Right(SecurityConstants.Edit), HttpPost("create-agent")]
    [ProducesResponseType<AgentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AgentDto>> CreateAgent([FromBody] SaveAgentRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await agentService.CreateAgentAsync(request, cancellationToken));
    }

    /// <summary>
    /// Updates an agent.
    /// </summary>
    [Right(SecurityConstants.Edit), HttpPut("update-agent/{id:int}")]
    [ProducesResponseType<AgentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgentDto>> UpdateAgent(int id, [FromBody] SaveAgentRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await agentService.UpdateAgentAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Deletes an agent.
    /// </summary>
    [Right(SecurityConstants.Delete), HttpDelete("delete-agent/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAgent(int id, CancellationToken cancellationToken)
    {
        await agentService.DeleteAgentAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Returns all skills.
    /// </summary>
    [Right(SecurityConstants.Read), HttpGet("get-skills")]
    [ProducesResponseType<List<SkillDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<SkillDto>>> GetSkills(CancellationToken cancellationToken)
    {
        return Ok(await agentService.GetSkillsAsync(cancellationToken));
    }

    /// <summary>
    /// Creates a skill.
    /// </summary>
    [Right(SecurityConstants.Edit), HttpPost("create-skill")]
    [ProducesResponseType<SkillDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SkillDto>> CreateSkill([FromBody] SaveSkillRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await agentService.CreateSkillAsync(request, cancellationToken));
    }

    /// <summary>
    /// Updates a skill. Running agents keep the version they loaded.
    /// </summary>
    [Right(SecurityConstants.Edit), HttpPut("update-skill/{id:int}")]
    [ProducesResponseType<SkillDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkillDto>> UpdateSkill(int id, [FromBody] SaveSkillRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await agentService.UpdateSkillAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Deletes a skill; it is removed from the agents.
    /// </summary>
    [Right(SecurityConstants.Delete), HttpDelete("delete-skill/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSkill(int id, CancellationToken cancellationToken)
    {
        await agentService.DeleteSkillAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Returns the tools of the tool catalog that skills can give agents.
    /// </summary>
    [Right(SecurityConstants.Read), HttpGet("get-tools")]
    [ProducesResponseType<List<AgentToolDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public ActionResult<List<AgentToolDto>> GetTools()
    {
        return Ok(agentService.GetTools());
    }

    /// <inheritdoc />
    public override List<SecurityResponse> GetModuleRights()
    {
        return
        [
            new SecurityResponse
            {
                Code = SecurityConstants.Read,
                Name = "Read",
                Description = "Can view agents, skills and tools",
                Roles = [SecurityConstants.AdminRole]
            },
            new SecurityResponse
            {
                Code = SecurityConstants.Edit,
                Name = "Edit",
                Description = "Can create and edit agents and skills",
                Roles = [SecurityConstants.AdminRole]
            },
            new SecurityResponse
            {
                Code = SecurityConstants.Delete,
                Name = "Delete",
                Description = "Can delete agents and skills",
                Roles = [SecurityConstants.AdminRole]
            }
        ];
    }
}
