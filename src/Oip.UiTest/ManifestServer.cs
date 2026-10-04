using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Oip.UiTest;

/// <summary>
/// Serves the manifest of an extension module over HTTP, so that a test can register the module: the backend
/// downloads the manifest from the given address. The module itself is never loaded, so nothing else is served.
/// The backend reaches the server through <see cref="TestSetup.TestHost"/>.
/// </summary>
internal sealed class ManifestServer : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly string _manifest;

    /// <summary>
    /// Starts the server for an extension module with the given key and name.
    /// </summary>
    /// <param name="key">Unique key of the extension module.</param>
    /// <param name="name">Name of the module as the registry shows it.</param>
    public ManifestServer(string key, string name)
    {
        var port = GetFreePort();
        var origin = $"http://{TestSetup.TestHost}:{port}";
        ManifestUrl = $"{origin}/manifest.json";
        // The script and the API must share the origin of the manifest; the element name needs a dash.
        _manifest = JsonSerializer.Serialize(new
        {
            key,
            name,
            version = "1.0.0",
            loadType = "customElement",
            elementName = $"{key}-element",
            scriptUrl = $"{origin}/main.js",
            apiBaseUrl = $"{origin}/api"
        });

        // "+" listens on every interface: in the test container the backend comes from the docker network.
        _listener.Prefixes.Add($"http://+:{port}/");
        _listener.Start();
        _ = ServeAsync();
    }

    /// <summary>
    /// Address of the manifest as the backend sees it.
    /// </summary>
    public string ManifestUrl { get; }

    /// <summary>
    /// Stops the server.
    /// </summary>
    public void Dispose() => _listener.Close();

    private async Task ServeAsync()
    {
        while (_listener.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (Exception e) when (e is HttpListenerException or ObjectDisposedException)
            {
                return;
            }

            var found = context.Request.Url?.AbsolutePath == "/manifest.json";
            var body = Encoding.UTF8.GetBytes(found ? _manifest : string.Empty);
            context.Response.StatusCode = found ? 200 : 404;
            context.Response.ContentType = "application/json";
            await context.Response.OutputStream.WriteAsync(body);
            context.Response.Close();
        }
    }

    private static int GetFreePort()
    {
        using var socket = new TcpListener(IPAddress.Loopback, 0);
        socket.Start();
        return ((IPEndPoint)socket.LocalEndpoint).Port;
    }
}
