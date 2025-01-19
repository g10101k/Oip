﻿using System.Net;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog.Web;
using Oip.Base.Clients;
using Oip.Base.Services;
using Oip.Base.Settings;
using Oip.Data.Contexts;
using Polly;
using Polly.Extensions.Http;


namespace Oip.Base.Extensions;

/// <summary>
/// Oip wrapper for <see cref="WebApplicationBuilder"/>
/// </summary>
public static class OipModuleApplication
{
    private const string Bearer = "Bearer";

    /// <summary>
    /// Initializes a new instance of the WebApplicationBuilder class with preconfigured defaults
    /// </summary>
    /// <param name="settings">App settings</param>
    /// <returns></returns>
    public static WebApplicationBuilder CreateModuleBuilder(IBaseOipModuleAppSettings settings)
    {
        var builder = WebApplication.CreateBuilder(settings.AppSettingsOptions.ProgrammeArguments);
        builder.AddHttpClients(settings);
        builder.AddDefaultHealthChecks();
        builder.AddDefaultAuthentication(settings);
        builder.AddOpenApi(settings);
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
        builder.Logging.ClearProviders();
        builder.Host.UseNLog();
        builder.AddDefaultHealthChecks();
        builder.AddDefaultAuthentication(settings);
        builder.AddOpenApi(settings);
        builder.Services.AddData(settings.ConnectionString);
        builder.AddKeycloakClients(settings);
        builder.AddServices(settings);
        return builder;
    }

    private static void AddServices(this WebApplicationBuilder builder, IBaseOipModuleAppSettings settings)
    {
        builder.Services.AddSingleton(settings);
        builder.Services.AddScoped<KeycloakService>();
        builder.Services.AddScoped<UserService>();
        builder.Services.AddCors();
        builder.Services.AddControllersWithViews();
        builder.Services.AddMvc().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition
                = JsonIgnoreCondition.WhenWritingNull;
        });
    }


    private static void AddHttpClients(this WebApplicationBuilder builder, IBaseOipModuleAppSettings settings)
    {
        builder.Services.AddHttpClient<OipClient>(x => { x.BaseAddress = new Uri(settings.OipUrls); })
            .AddPolicyHandler(GetRetryPolicy());
        builder.AddKeycloakClients(settings);
    }

    private static void AddKeycloakClients(this WebApplicationBuilder builder, IBaseOipModuleAppSettings settings)
    {
        builder.Services.AddHttpClient<KeycloakClient>(x =>
            {
                x.BaseAddress = new Uri(settings.SecurityService.BaseUrl);
            })
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

            options.AddSecurityDefinition(Bearer, new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = Bearer
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = Bearer
                        },
                        Scheme = "oauth2",
                        Name = Bearer,
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
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

    private static void MapDefaultEndpoints(this WebApplication app)
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
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.MapDefaultEndpoints();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors(options => options.AllowAnyOrigin());
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller}/{action=Index}/{id?}");
        app.MapOpenApi(settings);
        app.MapFallbackToFile("index.html");

        OipContext.MigrateDb(settings.Provider, settings.NormalizedConnectionString);
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