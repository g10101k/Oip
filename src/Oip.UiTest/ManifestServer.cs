using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Oip.UiTest;

/// <summary>
/// Serves the manifest of an extension module over HTTP, so that a test can register the module: the backend
/// downloads the manifest from the given address. The module itself is never loaded, so nothing else is served.
/// The backend reaches the server through <see cref="TestSetup.TestHost"/>.
/// </summary>
internal sealed class ManifestServer : IDisposable
{
    private readonly WebApplication _app;

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
        var manifest = JsonSerializer.Serialize(new
        {
            key,
            name,
            version = "1.0.0",
            loadType = "customElement",
            elementName = $"{key}-element",
            scriptUrl = $"{origin}/main.js",
            apiBaseUrl = $"{origin}/api"
        });

        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        // Every interface: in the test container the backend comes from the docker network. Unlike HttpListener
        // with a "+" prefix, Kestrel needs no URL reservation, so the tests also run without admin rights on Windows.
        builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(port));
        _app = builder.Build();
        _app.MapGet("/manifest.json", () => Results.Text(manifest, "application/json"));
        _app.StartAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Address of the manifest as the backend sees it.
    /// </summary>
    public string ManifestUrl { get; }

    /// <summary>
    /// Stops the server.
    /// </summary>
    public void Dispose() => _app.DisposeAsync().AsTask().GetAwaiter().GetResult();

    private static int GetFreePort()
    {
        using var socket = new TcpListener(IPAddress.Loopback, 0);
        socket.Start();
        return ((IPEndPoint)socket.LocalEndpoint).Port;
    }
}
