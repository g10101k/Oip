using System.Text.Json;

namespace Oip.UiTest;

/// <summary>
/// Calls the backend API from the page loaded in the browser, with the cookies of its session, to prepare
/// test data that has no UI. Changing requests carry the CSRF token, as the application does.
/// </summary>
internal class BackendApi(IWebDriver driver)
{
    private const string Script = """
        const [method, path, body, done] = arguments;
        (async () => {
          const headers = {};
          if (method !== 'GET') {
            const csrf = await fetch(new URL('api/security/get-auth-csrf-token', document.baseURI), { credentials: 'include' })
              .then(response => response.json());
            headers[csrf.headerName] = csrf.token;
          }
          if (body !== null) headers['Content-Type'] = 'application/json';
          const response = await fetch(new URL(path, document.baseURI), {
            method, headers, credentials: 'include', body: body === null ? undefined : body
          });
          const text = await response.text();
          done(response.ok ? { ok: true, text } : { ok: false, text: response.status + ' ' + text });
        })().catch(error => done({ ok: false, text: String(error) }));
        """;

    /// <summary>
    /// Sends a GET request and returns the JSON response.
    /// </summary>
    /// <param name="path">API path relative to the application root, e.g. <c>api/menu/get</c>.</param>
    public JsonElement Get(string path) => Parse(Send("GET", path, null));

    /// <summary>
    /// Sends a GET request and reports whether the backend accepted it, e.g. whether the session is still valid.
    /// </summary>
    /// <param name="path">API path relative to the application root.</param>
    /// <param name="response">The JSON response if the request succeeded.</param>
    public bool TryGet(string path, out JsonElement response)
    {
        var (ok, text) = Execute("GET", path, null);
        response = ok ? Parse(text) : default;
        return ok;
    }

    /// <summary>
    /// Sends a POST request with a JSON body and returns the JSON response, if any.
    /// </summary>
    /// <param name="path">API path relative to the application root.</param>
    /// <param name="body">Object serialized as the request body, or null to send none.</param>
    public JsonElement Post(string path, object? body = null) => Parse(Send("POST", path, body));

    /// <summary>
    /// Sends a DELETE request.
    /// </summary>
    /// <param name="path">API path relative to the application root.</param>
    public void Delete(string path) => Send("DELETE", path, null);

    private string Send(string method, string path, object? body)
    {
        var (ok, text) = Execute(method, path, body);
        return ok ? text : throw new InvalidOperationException($"{method} {path} failed: {text}");
    }

    private (bool Ok, string Text) Execute(string method, string path, object? body)
    {
        var json = body is null ? null : JsonSerializer.Serialize(body, JsonSerializerOptions.Web);
        var result = (Dictionary<string, object>)((IJavaScriptExecutor)driver).ExecuteAsyncScript(Script, method, path, json);
        return ((bool)result["ok"], (string)result["text"]);
    }

    private static JsonElement Parse(string text) =>
        string.IsNullOrWhiteSpace(text) ? default : JsonDocument.Parse(text).RootElement.Clone();
}
