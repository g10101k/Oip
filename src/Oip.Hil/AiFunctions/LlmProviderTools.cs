using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Oip.Base.Helpers;
using Oip.Hil.Data.Contexts;

namespace Oip.Hil.AiFunctions;

/// <summary>
/// Agent tools of an LLM request: the LLM provider configuration and the selection of the step outcome. Registered
/// as scoped: inside a Temporal activity the tools use the scope of the activity call, so the outcome selected in
/// one agent run is not shared with other runs. API keys are never exposed.
/// </summary>
public class LlmProviderTools(LlmContext context)
{
    /// <summary>
    /// Name of the <see cref="SelectOutcome"/> tool.
    /// </summary>
    public static readonly string SelectOutcomeToolName = ToolName(nameof(SelectOutcome));

    private const int MaxSelectAttempts = 3;

    private IReadOnlyList<string> _outcomes = [];
    private int _selectAttempts;

    /// <summary>
    /// Outcome selected by the model with <see cref="SelectOutcome"/>; <c>null</c> until a valid one is selected.
    /// </summary>
    public string? SelectedOutcome { get; private set; }

    /// <summary>
    /// Reason the model gave for <see cref="SelectedOutcome"/>.
    /// </summary>
    public string? SelectedReason { get; private set; }

    /// <summary>
    /// LLM provider as seen by the model.
    /// </summary>
    /// <param name="Name">Provider name.</param>
    /// <param name="Model">Default model of the provider.</param>
    /// <param name="IsDefault">Whether the provider is the default one.</param>
    public record ProviderInfo(string Name, string Model, bool IsDefault);

    /// <summary>
    /// Returns the tools for an agent run and resets the outcome selection. <see cref="SelectOutcome"/> is added
    /// only when <paramref name="outcomes"/> are set; its <c>outcome</c> parameter is limited to them.
    /// </summary>
    /// <param name="outcomes">Outcomes the model selects from; empty when no outcome is needed.</param>
    public IList<AITool> AsAiTools(IReadOnlyList<string> outcomes)
    {
        _outcomes = outcomes;
        _selectAttempts = 0;
        SelectedOutcome = null;
        SelectedReason = null;

        List<AITool> tools =
        [
            AIFunctionFactory.Create(GetEnabledProvidersAsync, ToolName(nameof(GetEnabledProvidersAsync)))
        ];
        if (outcomes.Count > 0)
            tools.Add(new OutcomeLimitedFunction(
                AIFunctionFactory.Create(SelectOutcome, SelectOutcomeToolName), outcomes));
        return tools;
    }

    /// <summary>
    /// Tool name for a method: snake_case without the <c>Async</c> suffix, e.g. <c>get_enabled_providers</c>.
    /// </summary>
    private static string ToolName(string methodName) =>
        (methodName.EndsWith("Async", StringComparison.Ordinal) ? methodName[..^"Async".Length] : methodName)
        .ToSnakeCase();

    /// <summary>
    /// Returns the enabled providers, the default one first, optionally filtered by a part of the name.
    /// </summary>
    /// <param name="name">Part of the provider name, case-insensitive; empty for all providers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [Description("Returns the enabled LLM providers with their default models; the default provider is first.")]
    public async Task<List<ProviderInfo>> GetEnabledProvidersAsync(
        [Description("Part of the provider name to search for, case-insensitive. Omit to return all providers.")]
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Providers.AsNoTracking().Where(x => x.IsEnabled);
        if (!string.IsNullOrWhiteSpace(name))
        {
            // ToLower is translated by both PostgreSQL and SQL Server, unlike a comparison with StringComparison.
            var pattern = name.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(pattern));
        }

        return await query
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .Select(x => new ProviderInfo(x.Name, x.Model, x.IsDefault))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Selects the outcome of the step. An unknown outcome is reported back to the model so it can call the tool
    /// again; a valid one, or too many failed attempts, ends the agent run.
    /// </summary>
    /// <param name="outcome">Selected outcome.</param>
    /// <param name="reason">Short explanation of the choice.</param>
    [Description("Select the outcome of the step and explain the choice.")]
    public string SelectOutcome(
        [Description("Selected outcome.")] string outcome,
        [Description("Short explanation of the choice.")] string reason)
    {
        var selected = _outcomes.FirstOrDefault(x => string.Equals(x, outcome?.Trim(), StringComparison.OrdinalIgnoreCase));
        var attempts = Interlocked.Increment(ref _selectAttempts);

        if (selected is not null)
        {
            SelectedOutcome = selected;
            SelectedReason = reason?.Trim();
        }

        // Stop the agent loop once the outcome is known or the model keeps failing.
        if ((selected is not null || attempts >= MaxSelectAttempts) &&
            FunctionInvokingChatClient.CurrentContext is { } invocation)
            invocation.Terminate = true;

        return selected is not null
            ? $"Outcome '{selected}' selected."
            : $"Unknown outcome '{outcome}'. Call {SelectOutcomeToolName} with one of: {string.Join(", ", _outcomes)}.";
    }

    /// <summary>
    /// <see cref="SelectOutcome"/> with the <c>outcome</c> parameter limited to the outcomes of the step by an
    /// <c>enum</c> in the schema. The schema is patched after it is generated: the schema generator does not tell
    /// which parameter a node belongs to.
    /// </summary>
    private sealed class OutcomeLimitedFunction(AIFunction inner, IReadOnlyList<string> outcomes)
        : DelegatingAIFunction(inner)
    {
        public override JsonElement JsonSchema { get; } = LimitOutcomes(inner.JsonSchema, outcomes);

        private static JsonElement LimitOutcomes(JsonElement schema, IReadOnlyList<string> outcomes)
        {
            var node = JsonNode.Parse(schema.GetRawText())!;
            // Name of the outcome parameter of SelectOutcome.
            node["properties"]!["outcome"]!["enum"] =
                new JsonArray(outcomes.Select(x => (JsonNode)JsonValue.Create(x)).ToArray());
            return JsonElement.Parse(node.ToJsonString());
        }
    }
}
