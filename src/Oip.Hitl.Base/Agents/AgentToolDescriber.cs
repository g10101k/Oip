using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Temporalio.Activities;

namespace Oip.Hitl.Base.Agents;

/// <summary>
/// Describes the tools of activity types: their methods marked with <see cref="AgentToolAttribute"/>.
/// </summary>
public static class AgentToolDescriber
{
    /// <summary>
    /// Returns the tools of the activity types, ordered by name.
    /// </summary>
    /// <param name="activityTypes">Activity types registered in the worker.</param>
    /// <param name="taskQueue">Task queue of the worker; <c>null</c> for the queue of the agent workflow.</param>
    /// <exception cref="InvalidOperationException">A tool is declared twice or has an unsupported signature.</exception>
    public static IReadOnlyList<AgentToolDefinition> Describe(IEnumerable<Type> activityTypes, string? taskQueue)
    {
        var tools = new Dictionary<string, AgentToolDefinition>(StringComparer.Ordinal);
        foreach (var type in activityTypes)
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            if (method.GetCustomAttribute<AgentToolAttribute>() is not { } tool) continue;
            var definition = Describe(method, tool, taskQueue);
            if (!tools.TryAdd(definition.Name, definition))
                throw new InvalidOperationException($"Agent tool '{definition.Name}' is declared more than once");
        }

        return tools.Values.OrderBy(x => x.Name, StringComparer.Ordinal).ToList();
    }

    private static AgentToolDefinition Describe(MethodInfo method, AgentToolAttribute tool, string? taskQueue)
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
            taskQueue,
            tool.TimeoutSeconds,
            tool.MaxAttempts,
            tool.RequiresApproval);
    }
}
