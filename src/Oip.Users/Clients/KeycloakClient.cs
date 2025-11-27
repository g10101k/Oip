using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Oip.Users.Clients;

/// <summary>
/// Keycloak client for authentication and user management with Keycloak server
/// </summary>
public sealed class KeycloakClient : HttpClient
{
    private readonly HttpClient _httpClient;
    private AuthResponse? _authResponse;


    private static readonly Lazy<JsonSerializerSettings> Settings = new(CreateSerializerSettings, true);

    private static JsonSerializerSettings CreateSerializerSettings()
    {
        var settings = new JsonSerializerSettings();
        UpdateJsonSerializerSettings(settings);
        return settings;
    }

    static void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        settings.NullValueHandling = NullValueHandling.Ignore;
        settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    }

    /// <summary>
    /// Json settings
    /// </summary>
    private JsonSerializerSettings JsonSerializerSettings => Settings.Value;

    /// <inheritdoc />
    public KeycloakClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Authenticates with Keycloak server using client credentials
    /// </summary>
    /// <param name="clientId">The client identifier</param>
    /// <param name="secret">The client secret</param>
    /// <param name="realm">The realm name</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Authentication response containing access token and expiration information</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<AuthResponse> Authentication(string clientId, string secret, string realm,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(clientId);
        ArgumentNullException.ThrowIfNull(secret);
        ArgumentNullException.ThrowIfNull(realm);
        var client = _httpClient;

        var param = new Dictionary<string, string>()
        {
            { "client_id", clientId },
            { "client_secret", secret },
            { "grant_type", "client_credentials" }
        };

        using var content = new FormUrlEncodedContent(param);

        using var request = new HttpRequestMessage();
        request.Content = content;
        request.RequestUri = new Uri($"realms/{realm}/protocol/openid-connect/token", UriKind.RelativeOrAbsolute);
        request.Method = new HttpMethod("POST");

        using var response = await client
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);


        var headers = new Dictionary<string, IEnumerable<string>>();
        foreach (var item in response.Headers)
            headers[item.Key] = item.Value;
        if (response.Content is { Headers: not null })
        {
            foreach (var item in response.Content.Headers)
                headers[item.Key] = item.Value;
        }

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var objectResponse =
                await ReadObjectResponseAsync<AuthResponse>(response, cancellationToken)
                    .ConfigureAwait(false);
            objectResponse.Object.ExpiresOn = DateTime.UtcNow.AddSeconds(objectResponse.Object.ExpiresIn);
            _authResponse = objectResponse.Object;
            if (objectResponse.Object == null)
            {
                throw new ApiException("Response was null which was not expected.", response.StatusCode,
                    objectResponse.Text,
                    headers, null);
            }

            return objectResponse.Object;
        }
        else
        {
            var responseData = response.Content == null
                ? null
                : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new ApiException($"The HTTP status code of the response was not expected ({response.StatusCode}).",
                response.StatusCode, responseData, headers, null);
        }
    }

    /// <summary>
    /// Gets all roles from the specified realm
    /// </summary>
    /// <param name="realm">The realm name</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<List<Role>> GetRoles(string realm, CancellationToken cancellationToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authResponse.AccessToken);

        using var request = new HttpRequestMessage();
        request.Method = new HttpMethod("GET");
        request.RequestUri = new Uri($"/admin/realms/{realm}/roles", UriKind.RelativeOrAbsolute);

        var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var objectResponse =
                await ReadObjectResponseAsync<List<Role>>(response, cancellationToken).ConfigureAwait(false);
            if (objectResponse.Object == null)
            {
                throw new ApiException("Response was null which was not expected.", response.StatusCode,
                    objectResponse.Text, null, null);
            }

            return objectResponse.Object;
        }
        else
        {
            var responseData = response.Content == null
                ? null
                : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new ApiException(
                "The HTTP status code of the response was not expected (" + response.StatusCode + ").",
                response.StatusCode,
                responseData, null, null);
        }
    }

    /// <summary>
    /// Object response result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private struct ObjectResponseResult<T>
    {
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="responseObject"></param>
        /// <param name="responseText"></param>
        public ObjectResponseResult(T responseObject, string responseText)
        {
            Object = responseObject;
            Text = responseText;
        }

        /// <summary>
        /// Response object
        /// </summary>
        public T Object { get; }

        /// <summary>
        /// Text response
        /// </summary>
        public string Text { get; }
    }

    /// <summary>
    /// Reads and deserializes the response content as an object of type T.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <param name="readResponseAsString">Whether to read the response content as a string before deserializing.</param>
    /// <typeparam name="T">The type of object to deserialize.</typeparam>
    /// <returns>An <see cref="ObjectResponseResult{T}"/> containing the deserialized object and the response text.</returns>
    /// <exception cref="ApiException">Thrown if deserialization fails.</exception>
    private async Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(
        HttpResponseMessage? response,
        CancellationToken cancellationToken, bool readResponseAsString = false)
    {
        if (response == null)
        {
            return new ObjectResponseResult<T>(default(T), string.Empty);
        }

        try
        {
            if (readResponseAsString)
            {
                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    var typedBody = JsonConvert.DeserializeObject<T>(responseText, JsonSerializerSettings);
                    return new ObjectResponseResult<T>(typedBody, responseText);
                }
                catch (JsonException exception)
                {
                    var message = "Could not deserialize the response body string as " + typeof(T).FullName + ".";
                    throw new ApiException(message, response.StatusCode, responseText,
                        new Dictionary<string, IEnumerable<string>>(), exception);
                }
            }

            await using var responseStream =
                await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var streamReader = new StreamReader(responseStream);
            await using var jsonTextReader = new JsonTextReader(streamReader);
            {
                var serializer = JsonSerializer.Create(JsonSerializerSettings);
                var typedBody = serializer.Deserialize<T>(jsonTextReader);
                return new ObjectResponseResult<T>(typedBody, string.Empty);
            }
        }
        catch (JsonException exception)
        {
            var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
            throw new ApiException(message, response.StatusCode, string.Empty,
                new Dictionary<string, IEnumerable<string>>(), exception);
        }
    }

    /// <summary>
    /// Gets a user by ID from the specified realm
    /// </summary>
    /// <param name="realm">Realm name</param>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User representation or null if not found</returns>
    public async Task<UserRepresentation?> GetUserAsync(string realm, string userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        using var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        request.RequestUri = new Uri($"/admin/realms/{realm}/users/{userId}", UriKind.RelativeOrAbsolute);


        using var response =
            await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var objectResponse = await ReadObjectResponseAsync<UserRepresentation>(response, cancellationToken);
            return objectResponse.Object;
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        else
        {
            await HandleErrorResponse(response, cancellationToken);
            return null;
        }
    }

    /// <summary>
    /// Gets users with pagination from the specified realm
    /// </summary>
    /// <param name="realm">Realm name</param>
    /// <param name="first">First result</param>
    /// <param name="max">Maximum results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of users</returns>
    public async Task<List<UserRepresentation>> GetUsersAsync(string realm, int first = 0, int max = 100,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        using var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        var uriBuilder = new UriBuilder($"{_httpClient.BaseAddress}/admin/realms/{realm}/users")
        {
            Query = $"first={first}&max={max}"
        };
        request.RequestUri = uriBuilder.Uri;

        using var response =
            await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var objectResponse = await ReadObjectResponseAsync<List<UserRepresentation>>(response, cancellationToken);
            return objectResponse.Object ?? new List<UserRepresentation>();
        }
        else
        {
            await HandleErrorResponse(response, cancellationToken);
            return new List<UserRepresentation>();
        }
    }

    /// <summary>
    /// Gets the count of users in the specified realm
    /// </summary>
    /// <param name="realm">Realm name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Users count</returns>
    public async Task<int> GetUsersCountAsync(string realm, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        using var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        request.RequestUri = new Uri($"/admin/realms/{realm}/users/count", UriKind.RelativeOrAbsolute);

        using var response =
            await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            return int.Parse(responseText);
        }
        else
        {
            await HandleErrorResponse(response, cancellationToken);
            return 0;
        }
    }

    /// <summary>
    /// Searches users in the specified realm by search term
    /// </summary>
    /// <param name="realm">Realm name</param>
    /// <param name="search">Search term</param>
    /// <param name="first">First result</param>
    /// <param name="max">Maximum results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of users</returns>
    public async Task<List<UserRepresentation>> SearchUsersAsync(string realm, string search, int first = 0,
        int max = 100, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        using var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        var uriBuilder = new UriBuilder($"/admin/realms/{realm}/users")
        {
            Query = $"search={Uri.EscapeDataString(search)}&first={first}&max={max}"
        };
        request.RequestUri = uriBuilder.Uri;

        using var response =
            await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var objectResponse = await ReadObjectResponseAsync<List<UserRepresentation>>(response, cancellationToken);
            return objectResponse.Object ?? new List<UserRepresentation>();
        }
        else
        {
            await HandleErrorResponse(response, cancellationToken);
            return new List<UserRepresentation>();
        }
    }

    /// <summary>
    /// Ensure client is authenticated
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
    {
        if (_authResponse == null || _authResponse.ExpiresOn <= DateTime.UtcNow.AddMinutes(-5))
        {
            throw new InvalidOperationException(
                "Keycloak client is not authenticated. Call Authentication method first.");
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authResponse.AccessToken);
    }

    /// <summary>
    /// Handle error response
    /// </summary>
    /// <param name="response">HTTP response</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task HandleErrorResponse(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var responseData = response.Content == null
            ? null
            : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        throw new ApiException(
            $"The HTTP status code of the response was not expected ({response.StatusCode}).",
            response.StatusCode,
            responseData ?? string.Empty,
            new Dictionary<string, IEnumerable<string>>(),
            null);
    }

    /// <summary>
    /// Authenticates with Keycloak server using client credentials
    /// </summary>
    /// <param name="clientId">Client ID</param>
    /// <param name="clientSecret">Client secret</param>
    /// <param name="realm">Realm name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response</returns>
    public async Task<AuthResponse> AuthenticateAsync(string clientId, string clientSecret, string realm,
        CancellationToken cancellationToken = default)
    {
        return await Authentication(clientId, clientSecret, realm, cancellationToken);
    }
}

/// <summary>
/// Exception thrown when API calls fail
/// </summary>
public partial class ApiException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="statusCode">The HTTP status code</param>
    /// <param name="response">The response content</param>
    /// <param name="headers">The response headers</param>
    /// <param name="innerException">The inner exception</param>
    public ApiException(string message, HttpStatusCode statusCode, string response,
        IReadOnlyDictionary<string, IEnumerable<string>> headers,
        Exception innerException)
        : base(
            message + "\n\nStatus: " + statusCode + "\nResponse: \n" + ((response == null)
                ? "(null)"
                : response.Substring(0, response.Length >= 512 ? 512 : response.Length)), innerException)
    {
        StatusCode = (int)statusCode;
        Response = response;
        Headers = headers;
    }

    /// <summary>
    /// Collection of HTTP response headers returned by the failed API request.
    /// </summary>
    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; set; }

    public int StatusCode { get; set; }

    /// <summary>
    /// Exception for API calls.
    /// </summary>
    public ApiException(string message, HttpStatusCode statusCode, string response, Exception innerException)
        : base(
            message + "\n\nStatus: " + statusCode + "\nResponse: \n" + ((response == null)
                ? "(null)"
                : response.Substring(0, response.Length >= 512 ? 512 : response.Length)), innerException)
    {
        StatusCode = (int)statusCode;
        Response = response;
    }

    public string? Response { get; set; }
}

/// <summary>
/// Authentication response containing access token and expiration information
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// The access token.
    /// </summary>
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    /// <summary>
    /// The number of seconds the access token is valid for.
    /// </summary>
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// The date and time the access token expires.
    /// </summary>
    public DateTimeOffset ExpiresOn { get; set; }

    /// <summary>
    /// The number of seconds the refresh token is valid for.
    /// </summary>
    [JsonProperty("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }

    /// <summary>
    /// The type of the token.
    /// </summary>
    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    /// <summary>
    /// The number of seconds before the access token is valid.
    /// </summary>
    [JsonProperty("not_before_policy")]
    public int NotBeforePolicy { get; set; }

    /// <summary>
    /// Gets or sets the scope.
    /// </summary>
    [JsonProperty("scope")]
    public string Scope { get; set; }
}

/// <summary>
/// Represents a role in Keycloak
/// </summary>
public class Role
{
    /// <summary>
    /// Role identifier.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Indicates whether the role is composite.
    /// </summary>
    public bool Composite { get; set; }

    /// <summary>
    /// Indicates whether the role is assigned to a client.
    /// </summary>
    public bool ClientRole { get; set; }

    /// <summary>
    /// The container ID associated with the role.
    /// </summary>
    public string ContainerId { get; set; } = null!;
}

/// <summary>
/// Represents a user in Keycloak
/// </summary>
public class UserRepresentation
{
    /// <summary>
    /// User identifier.
    /// </summary>
    [JsonProperty("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Username.
    /// </summary>
    [JsonProperty("username")]
    public string? Username { get; set; }

    /// <summary>
    /// Email address.
    /// </summary>
    [JsonProperty("email")]
    public string? Email { get; set; }

    /// <summary>
    /// First name.
    /// </summary>
    [JsonProperty("firstName")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name.
    /// </summary>
    [JsonProperty("lastName")]
    public string? LastName { get; set; }

    /// <summary>
    /// Indicating whether the user is enabled.
    /// </summary>
    [JsonProperty("enabled")]
    public bool? Enabled { get; set; }

    /// <summary>
    /// Indicating whether the email is verified.
    /// </summary>
    [JsonProperty("emailVerified")]
    public bool? EmailVerified { get; set; }

    /// <summary>
    /// User attributes.
    /// </summary>
    [JsonProperty("attributes")]
    public Dictionary<string, object>? Attributes { get; set; }

    /// <summary>
    /// Timestamp when the user was created.
    /// </summary>
    [JsonProperty("createdTimestamp")]
    public long? CreatedTimestamp { get; set; }

    /// <summary>
    /// Required actions for the user.
    /// </summary>
    [JsonProperty("requiredActions")]
    public List<string>? RequiredActions { get; set; }
}

/// <summary>
/// Response containing a list of users and count
/// </summary>
public class UsersResponse
{
    /// <summary>
    /// List of users.
    /// </summary>
    [JsonProperty("users")]
    public List<UserRepresentation>? Users { get; set; }

    /// <summary>
    /// Count of users.
    /// </summary>
    [JsonProperty("count")]
    public int Count { get; set; }
}