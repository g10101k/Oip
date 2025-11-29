using System.Globalization;
using System.Net;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog.Web;
using Oip.Base.Exceptions;
using Oip.Base.Helpers;
using Oip.Base.Middlewares;
using Oip.Base.Runtime;
using Oip.Base.Services;
using Oip.Base.Settings;
using Oip.Base.StartupTasks;
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
    [Obsolete("Use particle call ")]
    public static WebApplicationBuilder CreateModuleBuilder(IBaseOipModuleAppSettings settings)
    {
        var builder = WebApplication.CreateBuilder(settings.AppSettingsOptions.ProgramArguments);
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
    [Obsolete]
    public static WebApplicationBuilder CreateShellBuilder(IBaseOipModuleAppSettings settings)
    {
        var builder = WebApplication.CreateBuilder(settings.AppSettingsOptions.ProgramArguments);
        builder.AddNlog();
        builder.AddDefaultHealthChecks();
        builder.AddDefaultAuthentication(settings);
        builder.AddOpenApi(settings);
        builder.Services.AddStartupTask<SwaggerGenerateWebClientStartupTask>();
        builder.Services.AddStartupRunner();
        builder.Services.AddSingleton(settings);
        builder.Services.AddScoped<UserService>();
        builder.Services.AddCors();
        builder.AddControllersAndView();
        builder.AddLocalization();

        return builder;
    }

    /// <param name="builder">The WebApplicationBuilder instance</param>
    extension(WebApplicationBuilder builder)
    {
        /// <summary>
        /// Adds controllers and configures JSON options for the application builder
        /// </summary>
        /// <returns>The modified WebApplicationBuilder instance</returns>
        public WebApplicationBuilder AddControllersAndView()
        {
            builder.Services.AddControllers().AddJsonOptions(option =>
            {
                option.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            builder.Services.AddMvc().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
            return builder;
        }

        /// <summary>
        /// Adds NLog logging to the application
        /// </summary>
        public void AddNlog()
        {
            builder.Logging.ClearProviders();
            builder.Host.UseNLog();
        }

        /// <summary>
        /// Configures localization options for the application
        /// </summary>
        public void AddLocalization()
        {
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new List<CultureInfo>
                {
                    new("en"),
                    new("ru")
                };
                options.DefaultRequestCulture = new RequestCulture(culture: "en", uiCulture: "en");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });
        }

        /// <summary>
        /// Adds OpenAPI/Swagger support to the application builder
        /// </summary>
        /// <param name="settings">The application settings</param>
        public void AddOpenApi(IBaseOipModuleAppSettings settings)
        {
            var openApiSettings = settings.OpenApi;
            if (openApiSettings.All(x => !x.Publish))
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

                openApiSettings.ForEach(apiSettings =>
                {
                    if (apiSettings.Publish)
                    {
                        options.SwaggerDoc(apiSettings.Name, new OpenApiInfo
                        {
                            Version = apiSettings.Version,
                            Title = apiSettings.Title,
                            Description = apiSettings.Description,
                        });
                    }
                });

                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    if (docName == "v1")
                        return apiDesc.GroupName is null || apiDesc.GroupName == docName;

                    return apiDesc.GroupName == docName;
                });
            });
        }

        /// <summary>
        /// Add a default liveness check to ensure app is responsive
        /// </summary>
        public void AddDefaultHealthChecks()
        {
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
        }

        /// <summary>
        /// Configures default authentication using JWT Bearer scheme.
        /// </summary>
        /// <param name="settings">The base Oip module application settings.</param>
        /// <returns>The configured web application builder.</returns>
        public WebApplicationBuilder AddDefaultAuthentication(IBaseOipModuleAppSettings settings)
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    var list = new List<string>();

                    var urlWithRealm = settings.SecurityService.BaseUrl
                        .UrlAppend("realms")
                        .UrlAppend(settings.SecurityService.Realm);
                    list.Add(urlWithRealm);

                    var dockerInternalUrl = settings.SecurityService.DockerUrl?
                        .UrlAppend("realms")
                        .UrlAppend(settings.SecurityService.Realm);

                    if (dockerInternalUrl is not null)
                        list.Add(dockerInternalUrl);

                    options.MetadataAddress = (dockerInternalUrl ?? urlWithRealm)
                        .UrlAppend(".well-known/openid-configuration");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuers = list,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.FromSeconds(settings.SecurityService.ClockSkewSeconds)
                    };

                    if (builder.Environment.IsDevelopment() && dockerInternalUrl is not null)
                    {
                        options.TokenValidationParameters.IssuerSigningKeyResolver =
                            (token, securityToken, kid, parameters) =>
                            {
                                var handler = new HttpClientHandler
                                {
                                    ServerCertificateCustomValidationCallback =
                                        (message, cert, chain, errors) => true
                                };
                                var client = new HttpClient(handler);
                                var jwksUri = dockerInternalUrl.UrlAppend("/protocol/openid-connect/certs");
                                var jwksJson = client.GetStringAsync(jwksUri).Result;
                                var keys = JsonWebKeySet.Create(jwksJson).GetSigningKeys();
                                return keys;
                            };
                    }
                });
            builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformation>();
            builder.Services.AddAuthorization();
            return builder;
        }

        /// <summary>
        /// Build Oip Module application
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        [Obsolete]
        public WebApplication BuildApp(IBaseOipModuleAppSettings settings)
        {
            var app = builder.Build();
            app.AddRequestLocalization();
            app.AddExceptionHandler();
            app.MapDefaultEndpoints();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors(options => options.AllowAnyOrigin());
            app.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");
            app.MapOpenApi(settings);
            app.MapFallbackToFile("index.html");

            return app;
        }
    }

    /// <param name="app">The application builder</param>
    extension(WebApplication app)
    {
        /// <summary>
        /// Maps default health check endpoints for application monitoring
        /// </summary>
        public void MapDefaultEndpoints()
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
        /// Configures HTTP Strict Transport Security to help protect against man-in-the-middle attacks
        /// </summary>
        /// <returns></returns>
        public WebApplication AddHsts()
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            return app;
        }

        /// <summary>
        /// Configures request localization for the application
        /// </summary>
        /// <returns></returns>
        public WebApplication AddRequestLocalization()
        {
            var localizeOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
            if (localizeOptions != null) app.UseRequestLocalization(localizeOptions.Value);
            return app;
        }

        /// <summary>
        /// Configures the application to handle exceptions and return a JSON response
        /// </summary>
        public WebApplication AddExceptionHandler()
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";

                    var error = context.Features.Get<IExceptionHandlerFeature>();
                    if (error != null)
                    {
                        var ex = error.Error;
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(
                            new OipException(ex.Message, 500, app.Environment.IsDevelopment() ? ex.StackTrace : null),
                            Settings.Value
                        ));
                    }
                });
            });
            return app;
        }

        /// <summary>
        /// Configures and maps Open API endpoints for the application
        /// </summary>
        /// <param name="settings">The application settings</param>
        public void MapOpenApi(IBaseOipModuleAppSettings settings)
        {
            if (settings.OpenApi.All(x => !x.Publish))
                return;
            app.UseSwagger();
            app.UseSwaggerUI(swaggerUiOptions =>
            {
                swaggerUiOptions.EnableTryItOutByDefault();
                settings.OpenApi.ForEach(api =>
                {
                    if (api.Publish)
                    {
                        swaggerUiOptions.SwaggerEndpoint(api.Url, api.Name);
                    }
                });
            });
        }
    }

    private static readonly Lazy<JsonSerializerSettings> Settings = new(() => new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    }, true);

    
    /// <summary>
    /// Creates an asynchronous Polly retry policy for HTTP requests that handles transient errors and
    /// retries on <see cref="HttpStatusCode.NotFound"/> and <see cref="HttpStatusCode.InternalServerError"/>.
    /// The policy performs up to five retries with exponential back‑off (2ⁿ seconds).
    /// </summary>
    /// <returns>An <see cref="IAsyncPolicy{HttpResponseMessage}"/> that can be attached to an <see cref="HttpClient"/>.</returns>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.InternalServerError)
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}