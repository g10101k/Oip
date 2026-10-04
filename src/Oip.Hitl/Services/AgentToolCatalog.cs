using Oip.Hitl.Base.Agents;
using Oip.Hitl.Base.Workflows;

namespace Oip.Hitl.Services;

/// <summary>
/// Tools of the activities registered in the worker of Oip.Hitl itself: their methods marked with
/// <see cref="AgentToolAttribute"/>. Tools of skill workers are registered in the database instead.
/// </summary>
/// <exception cref="InvalidOperationException">A tool is declared twice or has an unsupported signature.</exception>
public class AgentToolCatalog(OipWorkflowOptions workflowOptions)
{
    private readonly Dictionary<string, AgentToolDefinition> _tools = AgentToolDescriber
        .Describe(workflowOptions.Activities, null)
        .ToDictionary(x => x.Name, StringComparer.Ordinal);

    /// <summary>
    /// All tools, ordered by name.
    /// </summary>
    public IReadOnlyList<AgentToolDefinition> Tools => _tools.Values.OrderBy(x => x.Name, StringComparer.Ordinal).ToList();

    /// <summary>
    /// Returns the tool with the name; <c>null</c> when there is none.
    /// </summary>
    public AgentToolDefinition? Find(string name) => _tools.GetValueOrDefault(name);
}
