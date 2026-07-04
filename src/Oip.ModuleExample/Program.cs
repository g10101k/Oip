using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/manifest.json", (HttpRequest request) =>
{
    var origin = $"{request.Scheme}://{request.Host}";
    return Results.Json(new
    {
        key = "oip-module-example",
        name = "OIP Module Example",
        version = "1.0.0",
        elementName = "oip-module-example",
        scriptUrl = $"{origin}/modules/oip-module-example.js",
        apiBaseUrl = $"{origin}",
        icon = "pi pi-th-large",
        description = "Example Web Component extension loaded by the main OIP application.",
        permissions = new[]
        {
            new
            {
                code = "read",
                name = "Read",
                description = "Can view the example module."
            }
        },
        settingsSchema = new
        {
            type = "object",
            properties = new
            {
                accent = new
                {
                    type = "string",
                    title = "Accent",
                    @default = "emerald"
                }
            }
        }
    });
});

app.MapGet("/api/status", (HttpContext context) =>
{
    var userName = context.User.Identity?.Name;
    return Results.Json(new
    {
        message = "Hello from Oip.ModuleExample backend",
        user = string.IsNullOrWhiteSpace(userName) ? "anonymous-or-proxied-user" : userName,
        serverTime = DateTimeOffset.UtcNow
    });
});

app.MapPost("/api/echo", async (HttpRequest request) =>
{
    using var payload = await JsonDocument.ParseAsync(request.Body);
    return Results.Json(new
    {
        received = payload.RootElement,
        serverTime = DateTimeOffset.UtcNow
    });
});

app.Run();
