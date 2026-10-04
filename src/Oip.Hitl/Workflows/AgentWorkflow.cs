using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Oip.Hitl.Base.Agents;
using Oip.Hitl.Base.Workflows;
using Oip.Hitl.Base.Workflows.Steps;
using Oip.Hitl.Services;
using Oip.Hitl.Workflows.Activities;
using Temporalio.Common;
using Temporalio.Exceptions;
using Temporalio.Workflows;

namespace Oip.Hitl.Workflows;

/// <summary>
/// Input of <see cref="AgentWorkflow"/>.
/// </summary>
/// <param name="Agent">Agent that answers.</param>
/// <param name="Messages">Chat history; the agent answers the last message.</param>
/// <param name="Settings">Request parameters sent to the chat completions API as is, e.g. <c>temperature</c>.</param>
/// <param name="StreamKey">Key of the <see cref="AgentEventStream"/> the answer is streamed to; <c>null</c> to not stream.</param>
/// <param name="UserName">Login of the user who sent the message.</param>
/// <param name="UserStepTimeout">How long the agent waits for the user to answer a question or allow a tool call;
/// <see cref="AgentWorkflow.DefaultUserStepTimeout"/> when <c>null</c>.</param>
public record AgentWorkflowInput(
    AgentSnapshot Agent,
    IReadOnlyList<AgentMessage> Messages,
    IReadOnlyDictionary<string, JsonElement>? Settings,
    string? StreamKey,
    string? UserName,
    TimeSpan? UserStepTimeout = null);

/// <summary>
/// Agent run started by the OpenAI-compatible gateway for a chat message. The model sees the descriptions of the
/// skills of the agent and loads the ones it needs with <see cref="LoadSkillToolName"/>, which gives it their
/// instructions and tools. Its tool calls are made by the workflow as activities, possibly on other workers, and
/// their results are sent back to the model until it answers without calling tools. A failed tool call is reported
/// to the model instead of failing the run. The answer is streamed to the gateway through
/// <see cref="AgentWorkflowInput.StreamKey"/>.
/// The model asks the user with <see cref="AskUserToolName"/>, and the user allows each call of a tool that
/// requires approval. Both are user steps, so besides the chat they are answered on the task page; only the runs
/// that wait for the user have steps.
/// </summary>
[Workflow]
public class AgentWorkflow : UserWorkflowBase
{
    /// <summary>
    /// Prefix of the workflow ids of the agent runs.
    /// </summary>
    public const string RunIdPrefix = "agent-";

    /// <summary>
    /// Name of the tool the model loads skills with.
    /// </summary>
    public const string LoadSkillToolName = "load_skill";

    /// <summary>
    /// Name of the tool the model asks the user with.
    /// </summary>
    public const string AskUserToolName = "ask_user";

    /// <summary>
    /// Memo key with the login of the user who started the run; only this user answers the steps in the chat.
    /// </summary>
    public const string UserMemo = "AgentUser";

    /// <summary>
    /// Outcome of an approval step that allows the tool call.
    /// </summary>
    public const string AllowOutcome = "Allow";

    /// <summary>
    /// Outcome of an approval step that denies the tool call.
    /// </summary>
    public const string DenyOutcome = "Deny";

    /// <summary>
    /// How long the agent waits for the user by default.
    /// </summary>
    public static readonly TimeSpan DefaultUserStepTimeout = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Turns of the model in a run; the last one is offered no tools, so the model has to answer.
    /// </summary>
    public const int MaxTurns = 16;

    /// <summary>
    /// Longest tool result sent to the model, in characters.
    /// </summary>
    public const int MaxToolResultLength = 32 * 1024;

    private readonly Dictionary<string, AgentToolDefinition> _tools = new(StringComparer.Ordinal);
    private readonly HashSet<string> _loadedSkills = new(StringComparer.Ordinal);
    private AgentWorkflowInput _input = null!;
    private string _stepKind = AgentStepKind.Question;

    private static readonly JsonElement AskUserSchema = JsonSerializer.SerializeToElement(new JsonObject
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["question"] = new JsonObject
            {
                ["type"] = "string",
                ["description"] = "The question, in the language of the user."
            },
            ["options"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject { ["type"] = "string" },
                ["description"] = "Answers the user chooses from; omit it to let the user answer freely."
            }
        },
        ["required"] = new JsonArray("question")
    });

    /// <summary>
    /// Runs the workflow and returns the answer of the agent: the texts of all turns.
    /// </summary>
    [WorkflowRun]
    public async Task<AgentTurnResult> RunAsync(AgentWorkflowInput input)
    {
        _input = input;
        Workflow.Logger.LogInformation("Agent {Agent} run for {User}: {Count} message(s)", input.Agent.Code,
            input.UserName, input.Messages.Count);

        var messages = new List<AgentMessage>();
        if (CreateSystemPrompt(input.Agent) is { } systemPrompt)
            messages.Add(new AgentMessage("system", systemPrompt));
        messages.AddRange(input.Messages);

        var texts = new List<string>();
        long? promptTokens = null, completionTokens = null;
        for (var turn = 1;; turn++)
        {
            var tools = turn < MaxTurns ? CreateToolDeclarations() : [];
            var result = await ChatTurnAsync(new AgentTurnRequest(input.Agent.ProviderId, messages, input.Settings,
                input.StreamKey, tools));
            if (!string.IsNullOrEmpty(result.Content)) texts.Add(result.Content);
            promptTokens = Sum(promptTokens, result.PromptTokens);
            completionTokens = Sum(completionTokens, result.CompletionTokens);

            if (result.ToolCalls is not { Count: > 0 } toolCalls || tools.Count == 0)
                return result with
                {
                    Content = string.Join("\n\n", texts),
                    PromptTokens = promptTokens,
                    CompletionTokens = completionTokens,
                    ToolCalls = null
                };

            messages.Add(new AgentMessage("assistant", result.Content, toolCalls));
            foreach (var call in toolCalls)
            {
                var output = await CallToolAsync(call);
                if (output.Length > MaxToolResultLength)
                    output = output[..MaxToolResultLength] + "\n[truncated]";
                messages.Add(new AgentMessage("tool", output, ToolCallId: call.Id));
            }
        }
    }

    private static Task<AgentTurnResult> ChatTurnAsync(AgentTurnRequest request)
    {
        return Workflow.ExecuteActivityAsync((LlmActivities activities) => activities.ChatTurnAsync(request),
            new ActivityOptions
            {
                StartToCloseTimeout = TimeSpan.FromMinutes(5),
                // A few heartbeat intervals of the activity.
                HeartbeatTimeout = TimeSpan.FromSeconds(30),
                RetryPolicy = new RetryPolicy { MaximumAttempts = 3 }
            });
    }

    /// <summary>
    /// Makes a tool call and returns its result, or the error, as the text for the model.
    /// </summary>
    private async Task<string> CallToolAsync(AgentToolCall call)
    {
        JsonElement arguments;
        try
        {
            arguments = JsonDocument.Parse(call.Arguments).RootElement.Clone();
        }
        catch (JsonException)
        {
            return $"Error: the arguments of {call.Name} are not a valid JSON object.";
        }

        if (call.Name == LoadSkillToolName)
            return await LoadSkillAsync(arguments);
        if (call.Name == AskUserToolName)
            return await AskUserAsync(arguments);
        if (!_tools.TryGetValue(call.Name, out var tool))
            return $"Error: tool {call.Name} is not available. Load the skill that provides it first.";
        if (tool.RequiresApproval && await GetApprovalErrorAsync(tool, call) is { } denied)
            return denied;

        try
        {
            var result = await Workflow.ExecuteActivityAsync<JsonElement>(tool.ActivityName,
                tool.HasArguments ? [arguments] : [],
                new ActivityOptions
                {
                    TaskQueue = tool.TaskQueue,
                    StartToCloseTimeout = TimeSpan.FromSeconds(tool.TimeoutSeconds),
                    RetryPolicy = new RetryPolicy { MaximumAttempts = tool.MaxAttempts }
                });
            return result.ValueKind == JsonValueKind.String ? result.GetString()! : result.GetRawText();
        }
        catch (ActivityFailureException e)
        {
            Workflow.Logger.LogWarning("Tool {Tool} failed: {Error}", call.Name, GetCause(e));
            return $"Error: {GetCause(e)}";
        }
    }

    /// <summary>
    /// Asks the user the question and returns the answer for the model.
    /// </summary>
    private async Task<string> AskUserAsync(JsonElement arguments)
    {
        var question = GetString(arguments, "question");
        if (string.IsNullOrWhiteSpace(question))
            return $"Error: call {AskUserToolName} with the question.";
        var options = arguments.ValueKind == JsonValueKind.Object &&
                      arguments.TryGetProperty("options", out var values) && values.ValueKind == JsonValueKind.Array
            ? values.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()!)
                .ToList()
            : [];

        var answer = await WaitForUserAsync(AgentStepKind.Question, new UserTaskStep
        {
            Title = $"Question from {_input.Agent.Code}",
            Description = question,
            Outcomes = options
        });
        if (answer is null)
            return "The user did not answer in time.";
        if (string.IsNullOrEmpty(answer.Result))
            return answer.Comment is null
                ? "The user declined to answer."
                : $"The user declined to answer: {answer.Comment}";
        return answer.Comment is null ? answer.Result : $"{answer.Result}\n\nComment: {answer.Comment}";
    }

    /// <summary>
    /// Asks the user to allow the tool call; returns the error for the model when the call is not allowed.
    /// </summary>
    private async Task<string?> GetApprovalErrorAsync(AgentToolDefinition tool, AgentToolCall call)
    {
        var approval = await WaitForUserAsync(AgentStepKind.Approval, new UserTaskStep
        {
            Title = $"Allow {tool.Name}?",
            Description = $"{tool.Description}\n\nArguments: {call.Arguments}",
            Outcomes = [AllowOutcome, DenyOutcome]
        });
        if (approval is null)
            return $"Error: the user did not allow the call of {tool.Name} in time, so it was not made.";
        if (approval.Result == AllowOutcome)
            return null;
        return approval.Comment is null
            ? $"Error: the user denied the call of {tool.Name}."
            : $"Error: the user denied the call of {tool.Name}: {approval.Comment}";
    }

    private Task<UserStepResult<string>?> WaitForUserAsync(string kind, UserTaskStep step)
    {
        _stepKind = kind;
        return UserStepAsync(step, _input.UserStepTimeout ?? DefaultUserStepTimeout);
    }

    /// <summary>
    /// Tells the gateway streaming the run that it waits for the user. Without it the step is still answered on
    /// the task page, so a failure does not fail the run.
    /// </summary>
    protected override async Task OnUserStepCreatedAsync(UserStep step)
    {
        if (_input.StreamKey is null) return;
        var request = new PublishUserStepRequest(_input.StreamKey, step.Id, _stepKind);
        try
        {
            // Not a local activity: the step is in the memo, which the gateway reads, only after the workflow task.
            await Workflow.ExecuteActivityAsync((AgentActivities activities) => activities.PublishUserStepAsync(request),
                new ActivityOptions
                {
                    StartToCloseTimeout = TimeSpan.FromSeconds(10),
                    RetryPolicy = new RetryPolicy { MaximumAttempts = 3 }
                });
        }
        catch (ActivityFailureException e)
        {
            Workflow.Logger.LogWarning("Failed to publish step {StepId}: {Error}", step.Id, GetCause(e));
        }
    }

    private async Task<string> LoadSkillAsync(JsonElement arguments)
    {
        var code = GetString(arguments, "name");
        if (code is null)
            return $"Error: call {LoadSkillToolName} with the name of a skill.";
        if (_loadedSkills.Contains(code))
            return $"Skill {code} is already loaded.";

        LoadedSkill skill;
        try
        {
            var request = new LoadSkillRequest(_input.Agent.AgentId, code);
            skill = await Workflow.ExecuteActivityAsync(
                (AgentActivities activities) => activities.LoadSkillAsync(request),
                new ActivityOptions
                {
                    StartToCloseTimeout = TimeSpan.FromSeconds(30),
                    RetryPolicy = new RetryPolicy { MaximumAttempts = 3 }
                });
        }
        catch (ActivityFailureException e)
        {
            return $"Error: {GetCause(e)}";
        }

        _loadedSkills.Add(skill.Code);
        foreach (var tool in skill.Tools)
            _tools[tool.Name] = tool;
        var tools = skill.Tools.Count == 0
            ? "The skill has no tools."
            : $"Tools now available: {string.Join(", ", skill.Tools.Select(x => x.Name))}.";
        return $"{skill.Instructions}\n\n{tools}";
    }

    private List<AgentToolDeclaration> CreateToolDeclarations()
    {
        var declarations = _tools.Values.OrderBy(x => x.Name, StringComparer.Ordinal)
            .Select(x => new AgentToolDeclaration(x.Name, x.Description, x.ParametersSchema))
            .ToList();
        declarations.Insert(0, new AgentToolDeclaration(AskUserToolName,
            "Asks the user a question and waits for the answer. Use it when only the user can give the information " +
            "or make the decision, not to confirm what you were asked to do.",
            AskUserSchema));
        var notLoaded = _input.Agent.Skills.Select(x => x.Code).Where(x => !_loadedSkills.Contains(x)).ToList();
        if (notLoaded.Count > 0)
            declarations.Insert(0, new AgentToolDeclaration(LoadSkillToolName,
                "Loads a skill: returns its instructions and makes its tools available.",
                CreateLoadSkillSchema(notLoaded)));
        return declarations;
    }

    private static JsonElement CreateLoadSkillSchema(IEnumerable<string> skills)
    {
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "Name of the skill.",
                    ["enum"] = new JsonArray(skills.Select(x => (JsonNode)x).ToArray())
                }
            },
            ["required"] = new JsonArray("name")
        };
        return JsonSerializer.SerializeToElement(schema);
    }

    private static string? CreateSystemPrompt(AgentSnapshot agent)
    {
        var prompt = new StringBuilder(agent.SystemPrompt);
        if (agent.Skills.Count > 0)
        {
            if (prompt.Length > 0) prompt.Append("\n\n");
            prompt.Append("You have skills: instructions and tools for particular tasks. Before a task a skill is " +
                          $"meant for, call {LoadSkillToolName} with its name and follow its instructions.\nSkills:");
            foreach (var skill in agent.Skills)
                prompt.Append($"\n- {skill.Code}: {skill.Description}");
        }

        return prompt.Length == 0 ? null : prompt.ToString();
    }

    /// <summary>
    /// Message of the innermost failure, e.g. the error of the tool instead of "Activity task failed".
    /// </summary>
    private static string GetCause(Exception exception)
    {
        var cause = exception;
        while (cause.InnerException is FailureException inner)
            cause = inner;
        return cause.Message;
    }

    private static string? GetString(JsonElement arguments, string property) =>
        arguments.ValueKind == JsonValueKind.Object && arguments.TryGetProperty(property, out var value) &&
        value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static long? Sum(long? total, long? value) => value is null ? total : (total ?? 0) + value;
}
