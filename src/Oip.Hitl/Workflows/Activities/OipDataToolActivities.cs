using System.ComponentModel;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Oip.Hitl.Base.Agents;
using Temporalio.Activities;
using Temporalio.Exceptions;

namespace Oip.Hitl.Workflows.Activities;

/// <summary>
/// Demo tools that read OIP data with the rights of the user who chats with the agent: they call the OIP API with the
/// token of the user, so the API decides what the user may see, as for the user in the browser.
/// </summary>
public class OipDataToolActivities(IHttpClientFactory httpClientFactory, IAgentUserTokenProvider userTokens)
{
    /// <summary>
    /// Name of the HTTP client of the users API.
    /// </summary>
    public const string UsersHttpClient = "Oip.Users";

    /// <summary>
    /// Name of the HTTP client of the notifications API.
    /// </summary>
    public const string NotificationsHttpClient = "Oip.Notifications";

    /// <summary>
    /// Arguments of <see cref="GetMyNotificationsAsync"/>.
    /// </summary>
    public class GetMyNotificationsArguments
    {
        /// <summary>
        /// Whether only unread notifications are returned.
        /// </summary>
        [JsonPropertyName("unread_only")]
        [Description("Return only unread notifications. True when omitted.")]
        public bool UnreadOnly { get; init; } = true;

        /// <summary>
        /// Number of notifications.
        /// </summary>
        [JsonPropertyName("take")]
        [Description("Number of the newest notifications to return, 1-50. 10 when omitted.")]
        public int Take { get; init; } = 10;
    }

    /// <summary>
    /// Arguments of <see cref="SearchUsersAsync"/>.
    /// </summary>
    public class SearchUsersArguments
    {
        /// <summary>
        /// Search term.
        /// </summary>
        [JsonPropertyName("term")]
        [Description("Part of the login, name or email, at least 2 characters.")]
        public string Term { get; init; } = string.Empty;
    }

    /// <summary>
    /// Returns the notifications of the user.
    /// </summary>
    [Activity, AgentTool("get_my_notifications", TimeoutSeconds = 30)]
    [Description("Returns the notifications of the current user in OIP, newest first, as JSON.")]
    public Task<string> GetMyNotificationsAsync(GetMyNotificationsArguments arguments) =>
        GetAsync(NotificationsHttpClient,
            $"api/notification/get-notification-by-user?take={Math.Clamp(arguments.Take, 1, 50)}" +
            $"&unreadOnly={(arguments.UnreadOnly ? "true" : "false")}", "read the notifications");

    /// <summary>
    /// Searches the users of OIP; only administrators may do it.
    /// </summary>
    [Activity, AgentTool("search_users", TimeoutSeconds = 30)]
    [Description("Searches the users of OIP by login, name or email and returns them as JSON. Only for " +
                 "administrators.")]
    public Task<string> SearchUsersAsync(SearchUsersArguments arguments) =>
        GetAsync(UsersHttpClient, $"api/users/search-user?term={Uri.EscapeDataString(arguments.Term.Trim())}",
            "search users");

    private async Task<string> GetAsync(string client, string path, string action)
    {
        var cancellationToken = ActivityExecutionContext.Current.CancellationToken;
        var token = await userTokens.GetAccessTokenAsync(cancellationToken: cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await httpClientFactory.CreateClient(client).SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode) return body;
        if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
            throw new ApplicationFailureException($"Access denied: the user has no right to {action} in OIP",
                "AccessDenied", nonRetryable: true);
        if ((int)response.StatusCode < 500)
            throw new ApplicationFailureException($"OIP rejected the request ({(int)response.StatusCode}): {body}",
                "InvalidArgument", nonRetryable: true);
        // A server error may be temporary, so the call is retried.
        throw new HttpRequestException($"OIP failed ({(int)response.StatusCode}): {body}");
    }
}
