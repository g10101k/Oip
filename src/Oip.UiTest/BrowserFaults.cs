using OpenQA.Selenium.Remote;

namespace Oip.UiTest;

/// <summary>
/// Makes the backend look broken to the page. Requests sent while the application starts can't be
/// intercepted from a script run after the page has loaded, so the fault is installed through the
/// Chrome DevTools protocol and applies to every page loaded afterwards. The CDP endpoint of ChromeDriver
/// is called directly, so it works both with a local driver and through Selenium Grid.
/// </summary>
internal sealed class BrowserFaults : IDisposable
{
    private const string CdpCommand = "oipExecuteCdpCommand";

    private readonly ICustomDriverCommandExecutor _executor;
    private readonly string _scriptId;

    private BrowserFaults(IWebDriver driver, string script)
    {
        _executor = (ICustomDriverCommandExecutor)driver;
        _executor.RegisterCustomDriverCommand(CdpCommand,
            new HttpCommandInfo(HttpCommandInfo.PostCommand, "/session/{sessionId}/goog/cdp/execute"));

        var result = (Dictionary<string, object>)ExecuteCdp("Page.addScriptToEvaluateOnNewDocument",
            new Dictionary<string, object> { ["source"] = script })!;
        _scriptId = (string)result["identifier"];
    }

    /// <summary>
    /// Answers the application requests whose address contains the given text with an internal server error,
    /// until the returned object is disposed. Takes effect from the next page load.
    /// </summary>
    /// <param name="driver">The web driver.</param>
    /// <param name="urlPart">Part of the request address, e.g. <c>api/user-profile/get-user-photo</c>.</param>
    public static BrowserFaults FailRequests(IWebDriver driver, string urlPart)
    {
        // The generated API client sends its requests with window.fetch.
        var script = $$"""
            (() => {
              const originalFetch = window.fetch;
              window.fetch = function (input, init) {
                const url = typeof input === 'string' ? input : (input && input.url) || String(input);
                if (url.includes('{{urlPart}}')) {
                  const body = JSON.stringify({ title: 'Failure injected by a UI test' });
                  return Promise.resolve(new Response(body, { status: 500, headers: { 'Content-Type': 'application/json' } }));
                }
                return originalFetch.apply(this, arguments);
              };
            })();
            """;
        return new BrowserFaults(driver, script);
    }

    /// <summary>
    /// Removes the fault. Pages loaded afterwards talk to the backend again.
    /// </summary>
    public void Dispose() =>
        ExecuteCdp("Page.removeScriptToEvaluateOnNewDocument",
            new Dictionary<string, object> { ["identifier"] = _scriptId });

    private object? ExecuteCdp(string command, Dictionary<string, object> parameters) =>
        _executor.ExecuteCustomDriverCommand(CdpCommand,
            new Dictionary<string, object> { ["cmd"] = command, ["params"] = parameters });
}
