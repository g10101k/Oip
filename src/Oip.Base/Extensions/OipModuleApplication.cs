﻿using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Oip.Base.Clients;
using Oip.Base.Services;
using Oip.Base.Settings;
using Polly;
using Polly.Extensions.Http;

namespace Oip.Base.Extensions;

/// <summary>
/// Oip wrapper for <see cref="WebApplicationBuilder"/>
/// </summary>
public static class OipModuleApplication
{
    /// <summary>
    /// Initializes a new instance of the WebApplicationBuilder class with preconfigured defaults
    /// </summary>
    /// <param name="settings">App settings</param>
    /// <returns></returns>
    public static WebApplicationBuilder CreateModuleBuilder(IBaseOipModuleAppSettings settings)
    {
        var builder = WebApplication.CreateBuilder(settings.AppSettingsOptions.ProgrammeArguments);
        builder.AddOipClient(settings);
        builder.AddDefaultHealthChecks();
        builder.AddDefaultAuthentication();
        builder.AddOpenApi(settings);
        builder.Services.AddHostedService<ModulesRegistryProcess>();
        builder.Services.AddControllersWithViews();
        builder.Services.AddSingleton(settings);
        return builder;
    }

    /// <summary>
    /// Initializes a new instance of the WebApplicationBuilder class with preconfigured defaults
    /// </summary>
    /// <param name="settings">App settings</param>
    /// <returns></returns>
    public static WebApplicationBuilder CreateShellBuilder(IBaseOipModuleAppSettings settings)
    {
        var builder = WebApplication.CreateBuilder(settings.AppSettingsOptions.ProgrammeArguments);
        builder.AddDefaultHealthChecks();
        builder.AddDefaultAuthentication();
        builder.AddOpenApi(settings);
        builder.Services.AddControllersWithViews();
        builder.Services.AddSingleton(settings);
        return builder;
    }

    private static void AddOipClient(this WebApplicationBuilder builder, IBaseOipModuleAppSettings settings)
    {
        builder.Services.AddHttpClient<OipClient>(x => { x.BaseAddress = new Uri(settings.OipUrls); })
            .AddPolicyHandler(GetRetryPolicy());
    }

    private static void AddOpenApi(this WebApplicationBuilder builder, IBaseOipModuleAppSettings settings)
    {
        var openApiSettings = settings.OpenApi;
        if (!openApiSettings.Publish)
            return;
        builder.Services.AddSwaggerGen(options =>
        {
            var path = Path.GetDirectoryName(typeof(OipModuleApplication).Assembly.Location);
            if (path == null) return;

            var filesPaths = Directory.GetFiles(path, "*.xml");
            foreach (var filePath in filesPaths)
            {
                options.IncludeXmlComments(filePath);
            }

            options.SwaggerDoc(openApiSettings.Name, new OpenApiInfo
            {
                Version = openApiSettings.Version,
                Title = openApiSettings.Title,
                Description = openApiSettings.Description,
            });
        });
    }  

    /// <summary>
    /// Add a default liveness check to ensure app is responsive
    /// </summary>
    /// <param name="builder"></param>
    private static void AddDefaultHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
    }

    private static void MapDefaultEndpoints(this WebApplication app, IBaseOipModuleAppSettings settings)
    {
        // All health checks must pass for app to be considered ready to accept traffic after starting
        app.MapHealthChecks("/health");

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        app.MapHealthChecks("/liveness", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });
    }

    /// <summary>
    /// Build Oip Module application
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static WebApplication BuildApp(this WebApplicationBuilder builder, IBaseOipModuleAppSettings settings)
    {
        var app = builder.Build();
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.MapDefaultEndpoints(settings);
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller}/{action=Index}/{id?}");
        app.MapOpenApi(settings);
        app.MapFallbackToFile("index.html");
        return app;
    }

    private static void MapOpenApi(this WebApplication app, IBaseOipModuleAppSettings settings)
    {
        if (!settings.OpenApi.Publish)
            return;
        app.UseSwagger();
        app.UseSwaggerUI(x => { x.EnableTryItOutByDefault(); });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.InternalServerError)
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}