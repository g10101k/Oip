using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Oip.Hitl.AiFunctions;
using Oip.Hitl.Base.Workflows;
using Temporalio.Activities;

namespace Oip.Hitl.Services;

/// <summary>
/// Tools of the activities registered in the worker: their methods marked with <see cref="AgentToolAttribute"/>.
/// </summary>
public class AgentToolCatalog
{
    private readonly Dictionary<string, AgentToolDefinition> _tools;

    /// <summary>
    /// Builds the catalog from the registered activities.
    /// </summary>
    /// <exception cref="InvalidOperationException">A tool is declared twice or has an unsupported signature.</exception>
    public AgentToolCatalog(OipWorkflowOptions workflowOptions)
    {
        _tools = new Dictionary<string, AgentToolDefinition>(StringComparer.Ordinal);
        foreach (var type in workflowOptions.Activities)
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            if (method.GetCustomAttribute<AgentToolAttribute>() is not { } tool) continue;
            var definition = CreateDefinition(method, tool);
            if (!_tools.TryAdd(definition.Name, definition))
                throw new InvalidOperationException($"Agent tool '{definition.Name}' is declared more than once");
        }
    }

    /// <summary>
    /// All tools, ordered by name.
    /// </summary>
    public IReadOnlyList<AgentToolDefinition> Tools => _tools.Values.OrderBy(x => x.Name).ToList();

    /// <summary>
    /// Returns the tool with the name; <c>null</c> when there is none.
    /// </summary>
    public AgentToolDefinition? Find(string name) => _tools.GetValueOrDefault(name);

    private static AgentToolDefinition CreateDefinition(MethodInfo method, AgentToolAttribute tool)
    {
        var where = $"{method.DeclaringType?.Name}.{method.Name}";
        if (!method.IsDefined(typeof(ActivityAttribute), true))
            throw new InvalidOperationException($"Agent tool {where} must be an activity");
        var parameters = method.GetParameters();
        if (parameters.Length > 1)
            throw new InvalidOperationException($"Agent tool {where} must take at most one parameter");
        if (method.ReturnType == typeof(void) || method.ReturnType == typeof(Task) || method.ReturnType == typeof(ValueTask))
            throw new InvalidOperationException($"Agent tool {where} must return a result for the model");

        // Temporal deserializes the arguments with the default serializer options, so the schema uses them too.
        var schema = parameters.Length == 0
            ? JsonDocument.Parse("""{"type":"object","properties":{}}""").RootElement.Clone()
            : AIJsonUtilities.CreateJsonSchema(parameters[0].ParameterType,
                serializerOptions: JsonSerializerOptions.Default);
        var activityName = ActivityDefinition.Create(method, _ => null).Name
                           ?? throw new InvalidOperationException($"Agent tool {where} has no activity name");

        return new AgentToolDefinition(
            tool.Name,
            method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty,
            schema,
            activityName,
            parameters.Length == 1,
            null,
            tool.TimeoutSeconds,
            tool.MaxAttempts,
            tool.RequiresApproval);
    }
}
