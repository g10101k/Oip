using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Oip.Base.Clients;

public class KeycloakClient : HttpClient
{
    private HttpClient _httpClient;
    private AuthResponse _authResponse;


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

    protected JsonSerializerSettings JsonSerializerSettings => Settings.Value;

    /// <inheritdoc />
    public KeycloakClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Registry module
    /// </summary>
    /// <param name="secret"></param>
    /// <param name="realm"></param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <param name="clientId"></param>
    /// <returns>Success</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual async Task<AuthResponse> Authentication(string clientId, string secret, string realm,
        CancellationToken cancellationToken)
    {
        if (clientId == null) throw new ArgumentNullException(nameof(clientId));
        if (secret == null) throw new ArgumentNullException(nameof(secret));
        if (realm == null) throw new ArgumentNullException(nameof(realm));
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
                await ReadObjectResponseAsync<AuthResponse>(response, cancellationToken, false)
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
    /// Get roles
    /// </summary>
    /// <param name="realm"></param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public virtual async Task<List<Role>> GetRoles(string realm, CancellationToken cancellationToken)
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
    protected struct ObjectResponseResult<T>
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
    /// 
    /// </summary>
    /// <param name="response"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="readResponseAsString"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ApiException"></exception>
    protected virtual async Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(
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
                    throw new ApiException(message, (int)response.StatusCode, responseText,
                        new Dictionary<string, IEnumerable<string>>(), exception);
                }
            }

            using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var streamReader = new StreamReader(responseStream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                var serializer = JsonSerializer.Create(JsonSerializerSettings);
                var typedBody = serializer.Deserialize<T>(jsonTextReader);
                return new ObjectResponseResult<T>(typedBody, string.Empty);
            }
        }
        catch (JsonException exception)
        {
            var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
            throw new ApiException(message, (int)response.StatusCode, string.Empty,
                new Dictionary<string, IEnumerable<string>>(), exception);
        }
    }
}

/// <summary>
/// Api exception
/// </summary>
public partial class ApiException : Exception
{
    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="message"></param>
    /// <param name="statusCode"></param>
    /// <param name="response"></param>
    /// <param name="headers"></param>
    /// <param name="innerException"></param>
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
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="statusCode"></param>
    /// <param name="response"></param>
    /// <param name="headers"></param>
    /// <param name="innerException"></param>
    public ApiException(string message, HttpStatusCode statusCode, string response, Exception innerException)
        : base(
            message + "\n\nStatus: " + statusCode + "\nResponse: \n" + ((response == null)
                ? "(null)"
                : response.Substring(0, response.Length >= 512 ? 512 : response.Length)), innerException)
    {
        StatusCode = (int)statusCode;
        Response = response;
    }
}

public class AuthResponse
{
    [JsonProperty("access_token")] public string AccessToken { get; set; }
    [JsonProperty("expires_in")] public int ExpiresIn { get; set; }
    public DateTimeOffset ExpiresOn { get; set; }
    [JsonProperty("refresh_expires_in")] public int RefreshExpiresIn { get; set; }
    [JsonProperty("token_type")] public string TokenType { get; set; }
    [JsonProperty("not_before_policy")] public int NotBeforePolicy { get; set; }
    [JsonProperty("scope")] public string Scope { get; set; }
}

public class Role
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Composite { get; set; }
    public bool ClientRole { get; set; }
    public string ContainerId { get; set; }
}