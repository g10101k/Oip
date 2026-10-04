using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Oip.Base.Exceptions;
using Oip.Hitl.Controllers.Api;
using Oip.Hitl.Data.Contexts;
using Oip.Hitl.Data.Entities;

namespace Oip.Hitl.Services;

/// <summary>
/// CRUD of agents and skills, and what agent runs read of them: the snapshot of an agent and its loaded skills.
/// </summary>
public partial class AgentService(LlmContext context, AgentToolCatalog toolCatalog, ILogger<AgentService> logger)
{
    /// <summary>
    /// Returns all agents ordered by name.
    /// </summary>
    public async Task<List<AgentDto>> GetAgentsAsync(CancellationToken cancellationToken)
    {
        var agents = await context.Agents.AsNoTracking().Include(x => x.Skills).OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
        return agents.Select(ToDto).ToList();
    }

    /// <summary>
    /// Creates an agent.
    /// </summary>
    public async Task<AgentDto> CreateAgentAsync(SaveAgentRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(request, null, cancellationToken);
        var now = DateTime.UtcNow;
        var agent = new AgentEntity { CreatedAt = now };
        Apply(agent, request, now);
        context.Agents.Add(agent);
        await context.SaveChangesAsync(cancellationToken);
        return ToDto(agent);
    }

    /// <summary>
    /// Updates an agent.
    /// </summary>
    public async Task<AgentDto> UpdateAgentAsync(int id, SaveAgentRequest request, CancellationToken cancellationToken)
    {
        var agent = await context.Agents.Include(x => x.Skills).FirstOrDefaultAsync(x => x.AgentId == id,
            cancellationToken) ?? throw AgentNotFound(id);
        await ValidateAsync(request, id, cancellationToken);
        Apply(agent, request, DateTime.UtcNow);
        await context.SaveChangesAsync(cancellationToken);
        return ToDto(agent);
    }

    /// <summary>
    /// Deletes an agent.
    /// </summary>
    public async Task DeleteAgentAsync(int id, CancellationToken cancellationToken)
    {
        var agent = await context.Agents.FirstOrDefaultAsync(x => x.AgentId == id, cancellationToken)
                    ?? throw AgentNotFound(id);
        context.Agents.Remove(agent);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Returns all skills ordered by code.
    /// </summary>
    public async Task<List<SkillDto>> GetSkillsAsync(CancellationToken cancellationToken)
    {
        var skills = await context.Skills.AsNoTracking().Include(x => x.Tools).OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);
        return skills.Select(ToDto).ToList();
    }

    /// <summary>
    /// Creates a skill.
    /// </summary>
    public async Task<SkillDto> CreateSkillAsync(SaveSkillRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(request, null, cancellationToken);
        var now = DateTime.UtcNow;
        var skill = new SkillEntity { CreatedAt = now };
        Apply(skill, request, now);
        context.Skills.Add(skill);
        await context.SaveChangesAsync(cancellationToken);
        return ToDto(skill);
    }

    /// <summary>
    /// Updates a skill. Running agents keep the version they loaded.
    /// </summary>
    public async Task<SkillDto> UpdateSkillAsync(int id, SaveSkillRequest request, CancellationToken cancellationToken)
    {
        var skill = await context.Skills.Include(x => x.Tools).FirstOrDefaultAsync(x => x.SkillId == id,
            cancellationToken) ?? throw SkillNotFound(id);
        await ValidateAsync(request, id, cancellationToken);
        Apply(skill, request, DateTime.UtcNow);
        await context.SaveChangesAsync(cancellationToken);
        return ToDto(skill);
    }

    /// <summary>
    /// Deletes a skill; it is removed from the agents.
    /// </summary>
    public async Task DeleteSkillAsync(int id, CancellationToken cancellationToken)
    {
        var skill = await context.Skills.FirstOrDefaultAsync(x => x.SkillId == id, cancellationToken)
                    ?? throw SkillNotFound(id);
        context.Skills.Remove(skill);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Returns the tools of the tool catalog.
    /// </summary>
    public List<AgentToolDto> GetTools() => toolCatalog.Tools
        .Select(x => new AgentToolDto(x.Name, x.Description, x.ParametersSchema.GetRawText()))
        .ToList();

    /// <summary>
    /// Returns the enabled agents ordered by name.
    /// </summary>
    public async Task<List<AgentDto>> GetEnabledAgentsAsync(CancellationToken cancellationToken)
    {
        return (await GetAgentsAsync(cancellationToken)).Where(x => x.IsEnabled).ToList();
    }

    /// <summary>
    /// Returns the enabled agent with the code and its enabled skills; <c>null</c> when there is none.
    /// </summary>
    public async Task<AgentSnapshot?> GetSnapshotAsync(string code, CancellationToken cancellationToken)
    {
        var agent = await context.Agents.AsNoTracking()
            .Where(x => x.Code == code && x.IsEnabled)
            .Select(x => new
            {
                x.AgentId,
                x.Code,
                x.LlmProviderId,
                x.SystemPrompt,
                Skills = x.Skills.Where(s => s.Skill.IsEnabled).OrderBy(s => s.Skill.Code)
                    .Select(s => new AgentSkillSummary(s.Skill.Code, s.Skill.Description)).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
        return agent is null
            ? null
            : new AgentSnapshot(agent.AgentId, agent.Code, agent.LlmProviderId, agent.SystemPrompt, agent.Skills);
    }

    /// <summary>
    /// Loads a skill of an agent: its instructions and the definitions of its tools. Tools missing from the catalog,
    /// e.g. removed from the code, are skipped.
    /// </summary>
    /// <exception cref="ApiException">The skill does not exist, is disabled or does not belong to the agent.</exception>
    public async Task<LoadedSkill> LoadSkillAsync(LoadSkillRequest request, CancellationToken cancellationToken)
    {
        var skill = await context.Skills.AsNoTracking().Include(x => x.Tools)
                        .FirstOrDefaultAsync(x => x.Code == request.Code && x.IsEnabled &&
                                                  x.Agents.Any(a => a.AgentId == request.AgentId), cancellationToken)
                    ?? throw new ApiException("Not found", $"Skill '{request.Code}' is not available to the agent",
                        StatusCodes.Status404NotFound);

        var tools = new List<AgentToolDefinition>();
        foreach (var name in skill.Tools.Select(x => x.ToolName).Order())
        {
            if (toolCatalog.Find(name) is { } tool)
                tools.Add(tool);
            else
                logger.LogWarning("Tool {Tool} of skill {Skill} is not in the tool catalog", name, skill.Code);
        }

        return new LoadedSkill(skill.Code, skill.Instructions, tools);
    }

    private async Task ValidateAsync(SaveAgentRequest request, int? id, CancellationToken cancellationToken)
    {
        ValidateCode(request.Code);
        if (string.IsNullOrWhiteSpace(request.Name))
            throw Invalid("Name is required");
        if (await context.Agents.AnyAsync(x => x.Code == request.Code.Trim() && x.AgentId != id, cancellationToken))
            throw Invalid($"Agent with code '{request.Code.Trim()}' already exists");
        if (request.LlmProviderId is { } providerId &&
            !await context.Providers.AnyAsync(x => x.LlmProviderId == providerId, cancellationToken))
            throw Invalid($"LLM provider {providerId} not found");

        var skillIds = request.SkillIds?.Distinct().ToList() ?? [];
        var found = await context.Skills.CountAsync(x => skillIds.Contains(x.SkillId), cancellationToken);
        if (found != skillIds.Count)
            throw Invalid("Some of the skills are not found");
    }

    private async Task ValidateAsync(SaveSkillRequest request, int? id, CancellationToken cancellationToken)
    {
        ValidateCode(request.Code);
        if (string.IsNullOrWhiteSpace(request.Description))
            throw Invalid("Description is required");
        if (string.IsNullOrWhiteSpace(request.Instructions))
            throw Invalid("Instructions are required");
        if (await context.Skills.AnyAsync(x => x.Code == request.Code.Trim() && x.SkillId != id, cancellationToken))
            throw Invalid($"Skill with code '{request.Code.Trim()}' already exists");

        var unknown = (request.Tools ?? []).Where(x => toolCatalog.Find(x) is null).ToList();
        if (unknown.Count > 0)
            throw Invalid($"Tools not found in the tool catalog: {string.Join(", ", unknown)}");
    }

    private static void ValidateCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code) || !CodePattern().IsMatch(code.Trim()))
            throw Invalid("Code must consist of lowercase letters, digits, dots, dashes and underscores");
    }

    private static void Apply(AgentEntity agent, SaveAgentRequest request, DateTime now)
    {
        agent.Code = request.Code.Trim();
        agent.Name = request.Name.Trim();
        agent.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        agent.SystemPrompt = string.IsNullOrWhiteSpace(request.SystemPrompt) ? null : request.SystemPrompt.Trim();
        agent.LlmProviderId = request.LlmProviderId;
        agent.IsEnabled = request.IsEnabled;
        agent.UpdatedAt = now;

        // Links are replaced by difference: removing and adding a link with the same key fails in EF.
        var skillIds = request.SkillIds?.ToHashSet() ?? [];
        agent.Skills.RemoveAll(x => !skillIds.Contains(x.SkillId));
        agent.Skills.AddRange(skillIds.Where(x => agent.Skills.All(s => s.SkillId != x))
            .Select(x => new AgentSkillEntity { SkillId = x }));
    }

    private static void Apply(SkillEntity skill, SaveSkillRequest request, DateTime now)
    {
        skill.Code = request.Code.Trim();
        skill.Description = request.Description.Trim();
        skill.Instructions = request.Instructions.Trim();
        skill.IsEnabled = request.IsEnabled;
        skill.UpdatedAt = now;

        var tools = request.Tools?.ToHashSet() ?? [];
        skill.Tools.RemoveAll(x => !tools.Contains(x.ToolName));
        skill.Tools.AddRange(tools.Where(x => skill.Tools.All(t => t.ToolName != x))
            .Select(x => new SkillToolEntity { ToolName = x }));
    }

    private static AgentDto ToDto(AgentEntity agent) => new(agent.AgentId, agent.Code, agent.Name, agent.Description,
        agent.SystemPrompt, agent.LlmProviderId, agent.IsEnabled, agent.Skills.Select(x => x.SkillId).Order().ToList(),
        agent.CreatedAt, agent.UpdatedAt);

    private static SkillDto ToDto(SkillEntity skill) => new(skill.SkillId, skill.Code, skill.Description,
        skill.Instructions, skill.IsEnabled, skill.Tools.Select(x => x.ToolName).Order().ToList(), skill.CreatedAt,
        skill.UpdatedAt);

    private static ApiException Invalid(string message) =>
        new("Validation error", message, StatusCodes.Status400BadRequest);

    private static ApiException AgentNotFound(int id) =>
        new("Not found", $"Agent {id} not found", StatusCodes.Status404NotFound);

    private static ApiException SkillNotFound(int id) =>
        new("Not found", $"Skill {id} not found", StatusCodes.Status404NotFound);

    [GeneratedRegex("^[a-z0-9][a-z0-9._-]*$")]
    private static partial Regex CodePattern();
}
