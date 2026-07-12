using System.Globalization;
using System.Net;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog.Web;
using Oip.Base.Clients;
using Oip.Base.Exceptions;
using Oip.Base.Helpers;
using Oip.Base.Middlewares;
using Oip.Base.Providers;
using Oip.Base.Runtime;
using Oip.Base.Services;
using Oip.Base.Settings;
using Oip.Base.StartupTasks;
using OpenTelemetry.Metrics;
using Polly;
using Polly.Extensions.Http;
using ApiException = Oip.Base.Exceptions.ApiException;

namespace Oip.Base.Extensions;

/// <summary>
/// Oip wrapper for <see cref="WebApplicationBuilder"/>
/// </summary>
public static class OipModuleApplication
{
    public const string CookieAuthenticationScheme = "OipCookie";
    public const string OpenIdConnectAuthenticationScheme = "OipOpenIdConnect";
    public const string DefaultAuthenticationScheme = "OipDefault";
    public const string CsrfHeaderName = "X-CSRF-TOKEN";
    private const string Bearer = "Bearer";

    /// <summary>
    /// Initializes a new instance of the WebApplicationBuilder class with preconfigured defaults
    /// </summary>
    /// <param name="settings">App settings</param>
    /// <returns></returns>
    [Obsolete("Use particle method call")]
    public static WebApplicationBuilder CreateModuleBuilder(ISettings settings)
    {
        var builder = WebApplication.CreateBuilder(settings.AppSettingsOptions.ProgramArguments);
        builder.Services.AddDefaultHealthChecks();
        builder.Services.AddForwardedHeaders(settings);
        builder.Services.AddDefaultAuthentication(settings);
        builder.Services.AddOpenApi(settings);
        builder.Services.AddControllersWithViews();
        builder.Services.AddSingleton(settings);
        return builder;
    }

    /// <summary>
    /// Initializes a new instance of the WebApplicationBuilder class with preconfigured defaults
    /// </summary>
    /// <param name="settings">App settings</param>
    /// <returns></returns>
    [Obsolete("Use particle method call")]
    public static WebApplicationBuilder CreateShellBuilder(ISettings settings)
    {
        var builder = WebApplication.CreateBuilder(settings.AppSettingsOptions.ProgramArguments);
        builder.AddNlog();
        builder.Services.AddDefaultHealthChecks();
        builder.Services.AddForwardedHeaders(settings);
        builder.Services.AddDefaultAuthentication(settings);
        builder.Services.AddOpenApi(settings);
        builder.Services.AddStartupTask<SwaggerGenerateWebClientStartupTask>();
        builder.Services.AddStartupRunner();
        builder.Services.AddSingleton(settings);
        builder.Services.AddScoped<ClaimService>();
        builder.Services.AddCors();
        builder.Services.AddControllersAndView();
        builder.Services.AddOipLocalization();

        return builder;
    }

    /// <summary>
    /// Adds controllers and configures JSON options for the application builder
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The modified service collection</returns>
    public static IServiceCollection AddControllersAndView(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(option =>
        {
            option.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        CommonMvcBuilderProducer(services);

        return services;
    }

    /// <summary>
    /// Используется для того чтобы не зависеть от порядка вызова AddControllersAndView и AddController
    /// </summary>
    private static IMvcBuilder CommonMvcBuilderProducer(IServiceCollection services)
    {
        IMvcBuilder mvcBuilder;
        var mvcBuilderHolderDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(MvcBuilderHolder));
        if (mvcBuilderHolderDescriptor?.ImplementationInstance is MvcBuilderHolder mvcBuilderHolder)
        {
            mvcBuilder = mvcBuilderHolder.Builder;
        }
        else
        {
            mvcBuilder = services.AddMvc().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
            services.AddSingleton(new MvcBuilderHolder(mvcBuilder));
        }

        return mvcBuilder;
    }

    /// <summary>
    /// Adds controllers and configures JSON options for the application builder
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance</param>
    /// <returns>The modified WebApplicationBuilder instance</returns>
    [Obsolete("Use extension for IServiceCollection")]
    public static WebApplicationBuilder AddControllersAndView(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllersAndView();
        return builder;
    }

    /// <summary>
    /// Adds NLog logging to the application
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance</param>
    public static void AddNlog(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Host.UseNLog();
    }

    /// <summary>
    /// Configures localization options for the application
    /// </summary>
    /// <param name="services">The service collection</param>
    public static IServiceCollection AddOipLocalization(this IServiceCollection services)
    {
        services.Configure<RequestLocalizationOptions>(options =>
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

        return services;
    }

    /// <summary>
    /// Configures localization options for the application
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance</param>
    [Obsolete("Use extension for IServiceCollection")]
    public static void AddLocalization(this WebApplicationBuilder builder)
    {
        builder.Services.AddOipLocalization();
    }

    /// <summary>
    /// Adds OpenAPI/Swagger support to the application builder
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <param name="services">The service collection</param>
    public static IServiceCollection AddOpenApi(this IServiceCollection services, ISettings settings)
    {
        var openApiSettings = settings.OpenApi;
        if (openApiSettings.All(x => !x.Publish))
            return services;
        services.AddSwaggerGen(options =>
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

        return services;
    }

    /// <summary>
    /// Adds OpenAPI/Swagger support to the application builder
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <param name="builder">The WebApplicationBuilder instance</param>
    [Obsolete("Use extension for IServiceCollection")]
    public static void AddOpenApi(this WebApplicationBuilder builder, ISettings settings)
    {
        builder.Services.AddOpenApi(settings);
    }

    /// <summary>
    /// Add a default liveness check to ensure app is responsive
    /// </summary>
    public static IServiceCollection AddDefaultHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return services;
    }

    /// <summary>
    /// Add a default liveness check to ensure app is responsive
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance</param>
    [Obsolete("Use extension for IServiceCollection")]
    public static void AddDefaultHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services.AddDefaultHealthChecks();
    }

    public static IServiceCollection AddOpenTelemetry(this IServiceCollection services,
        ISettings settings)
    {
        if (settings.OpenTelemetry.Enable)
            services.AddOpenTelemetry()
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter());

        return services;
    }

    [Obsolete("Use extension for IServiceCollection")]
    public static void AddOpenTelemetry(this WebApplicationBuilder builder, ISettings settings)
    {
        builder.Services.AddOpenTelemetry(settings);
    }

    /// <summary>
    /// Configures forwarded headers support from the ReverseProxy configuration section.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddForwardedHeaders(this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection("ReverseProxy").Get<ReverseProxySettings>() ?? new();
        return services.AddForwardedHeaders(settings);
    }

    /// <summary>
    /// Configures forwarded headers support from the ReverseProxy configuration section.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance</param>
    /// <returns>The configured web application builder.</returns>
    [Obsolete("Use extension for IServiceCollection")]
    public static WebApplicationBuilder AddForwardedHeaders(this WebApplicationBuilder builder)
    {
        builder.Services.AddForwardedHeaders(builder.Configuration);
        return builder;
    }

    /// <summary>
    /// Configures forwarded headers support.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="settings">The base Oip module application settings.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddForwardedHeaders(this IServiceCollection services,
        ISettings settings)
    {
        return services.AddForwardedHeaders(settings.ReverseProxy);
    }

    /// <summary>
    /// Configures forwarded headers support.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance</param>
    /// <param name="settings">The base Oip module application settings.</param>
    /// <returns>The configured web application builder.</returns>
    [Obsolete("Use extension for IServiceCollection")]
    public static WebApplicationBuilder AddForwardedHeaders(this WebApplicationBuilder builder,
        ISettings settings)
    {
        builder.Services.AddForwardedHeaders(settings);
        return builder;
    }

    private static IServiceCollection AddForwardedHeaders(this IServiceCollection services,
        ReverseProxySettings settings)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            if (!settings.Enabled)
            {
                options.ForwardedHeaders = ForwardedHeaders.None;
                return;
            }

            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto |
                ForwardedHeaders.XForwardedHost;

            options.ForwardLimit = Math.Max(1, settings.ForwardLimit);

            if (settings.TrustAllProxies)
            {
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
                return;
            }

            foreach (var proxy in settings.KnownProxies)
            {
                if (IPAddress.TryParse(proxy, out var address))
                    options.KnownProxies.Add(address);
            }

            foreach (var network in settings.KnownNetworks)
            {
                if (TryParseKnownNetwork(network, out var knownNetwork))
                    options.KnownNetworks.Add(knownNetwork);
            }
        });

        return services;
    }

    /// <summary>
    /// Configures default authentication using JWT Bearer scheme.
    /// </summary>
    /// <param name="settings">The base Oip module application settings.</param>
    /// <param name="services">The service collection</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddDefaultAuthentication(this IServiceCollection services,
        ISettings settings)
    {
        services.AddDataProtection()
            .SetApplicationName("OIP");
        services.AddAuthenticationTicketStore(settings.SecurityService.AuthTicketStore);
        services.AddAntiforgery(options =>
        {
            options.HeaderName = CsrfHeaderName;
            options.Cookie.Name = "__Host-OIP-CSRF";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = DefaultAuthenticationScheme;
                options.DefaultChallengeScheme = DefaultAuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationScheme;
            })
            .AddPolicyScheme(DefaultAuthenticationScheme, DefaultAuthenticationScheme, options =>
            {
                options.ForwardDefaultSelector = SelectDefaultAuthenticationScheme;
                options.ForwardChallenge = CookieAuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationScheme, options =>
            {
                options.Cookie.Name = "__Host-OIP";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(
                    Math.Max(1, settings.SecurityService.AuthTicketStore.TicketLifetimeMinutes));
                options.SlidingExpiration = true;
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context => WriteAuthenticationError(context.HttpContext, 401,
                        "Unauthorized", "Authentication session is required."),
                    OnRedirectToAccessDenied = context => WriteAuthenticationError(context.HttpContext, 403,
                        "Forbidden", "The current user does not have permission to access this resource."),
                    OnValidatePrincipal = context =>
                    {
                        ClaimsTransformation.AddRolesFromAccessToken(
                            context.Principal,
                            context.Properties.GetTokenValue("access_token"));
                        return Task.CompletedTask;
                    }
                };
            })
            .AddOpenIdConnect(OpenIdConnectAuthenticationScheme, options =>
            {
                var urlWithRealm = settings.SecurityService.BaseUrl
                    .UrlAppend("realms")
                    .UrlAppend(settings.SecurityService.Realm);

                var dockerInternalUrl = settings.SecurityService.DockerUrl?
                    .UrlAppend("realms")
                    .UrlAppend(settings.SecurityService.Realm);

                options.SignInScheme = CookieAuthenticationScheme;
                var backchannelUrlWithRealm = dockerInternalUrl ?? urlWithRealm;

                options.Authority = urlWithRealm;
                options.Configuration = CreateOpenIdConnectConfiguration(urlWithRealm, backchannelUrlWithRealm);
                options.ClientId = settings.SecurityService.Front.ClientId;
                if (string.Equals(options.ClientId, settings.SecurityService.ClientId, StringComparison.Ordinal))
                    options.ClientSecret = settings.SecurityService.ClientSecret;
                options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                options.UsePkce = true;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = false;
                options.ProtocolValidator = new OipOpenIdConnectProtocolValidator();
                options.Scope.Clear();
                options.Scope.Add(OpenIdConnectScope.OpenId);
                foreach (var scope in settings.SecurityService.Front.Scope.Split(' ',
                             StringSplitOptions.RemoveEmptyEntries))
                    if (!options.Scope.Contains(scope))
                        options.Scope.Add(scope);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuers = CreateValidIssuers(urlWithRealm, dockerInternalUrl),
                    IssuerSigningKeyResolver = (_, _, _, _) =>
                        GetSigningKeys(backchannelUrlWithRealm, settings.IsDevelopment()),
                    ClockSkew = TimeSpan.FromSeconds(settings.SecurityService.ClockSkewSeconds)
                };
                if (settings.IsDevelopment())
                {
                    options.BackchannelHttpHandler = CreateDevelopmentHttpClientHandler();
                }

                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.NonceCookie.SameSite = SameSiteMode.None;
                options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Events = new OpenIdConnectEvents
                {
                    OnTokenResponseReceived = context =>
                    {
                        var tokenResponse = context.TokenEndpointResponse;
                        if (!string.IsNullOrEmpty(tokenResponse.IdToken) ||
                            !string.IsNullOrEmpty(context.ProtocolMessage.IdToken))
                            return Task.CompletedTask;

                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger(nameof(OipModuleApplication));
                        logger.LogError(
                            "Keycloak token response for client {ClientId} did not contain id_token. " +
                            "HasAccessToken: {HasAccessToken}; HasRefreshToken: {HasRefreshToken}; " +
                            "TokenType: {TokenType}; Scope: {Scope}; Error: {Error}; ErrorDescription: {ErrorDescription}",
                            options.ClientId,
                            !string.IsNullOrEmpty(tokenResponse.AccessToken),
                            !string.IsNullOrEmpty(tokenResponse.RefreshToken),
                            tokenResponse.TokenType,
                            tokenResponse.Scope,
                            tokenResponse.Error,
                            tokenResponse.ErrorDescription);

                        var message = $"Keycloak token endpoint did not return an id_token for client " +
                                      $"'{options.ClientId}'. Ensure the client is an OpenID " +
                                      $"Connect client with standard flow enabled and that the requested scope " +
                                      $"contains '{OpenIdConnectScope.OpenId}'.";

                        throw new AuthenticationFailureException(message);
                    },
                    OnTicketReceived = context =>
                    {
                        ClaimsTransformation.AddRolesFromAccessToken(
                            context.Principal,
                            context.Properties?.GetTokenValue("access_token"));
                        return Task.CompletedTask;
                    }
                };
            })
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

                if (settings.IsDevelopment())
                {
                    options.BackchannelHttpHandler = CreateDevelopmentHttpClientHandler();
                }

                if (settings.IsDevelopment() && dockerInternalUrl is not null)
                {
                    options.TokenValidationParameters.IssuerSigningKeyResolver =
                        (token, securityToken, kid, parameters) =>
                        {
                            var handler = CreateDevelopmentHttpClientHandler();
                            var client = new HttpClient(handler);
                            var jwksUri = dockerInternalUrl.UrlAppend("/protocol/openid-connect/certs");
                            var jwksJson = client.GetStringAsync(jwksUri).Result;
                            var keys = JsonWebKeySet.Create(jwksJson).GetSigningKeys();
                            return keys;
                        };
                }

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                        {
                            var accessToken = context.Request.Query["access_token"];
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        services.AddTransient<IClaimsTransformation, ClaimsTransformation>();
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(DefaultAuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();
        });
        services.AddHttpClient<KeycloakClient>(x =>
                x.BaseAddress = new Uri(settings.SecurityService.DockerUrl ?? settings.SecurityService.BaseUrl))
            .ConfigurePrimaryHttpMessageHandler(() => settings.IsDevelopment()
                ? CreateDevelopmentHttpClientHandler()
                : new HttpClientHandler());
        services.AddScoped<ClaimService>();
        services.AddScoped<KeycloakService>();
        return services;
    }

    /// <summary>
    /// Configures default authentication using JWT Bearer scheme.
    /// </summary>
    /// <param name="settings">The base Oip module application settings.</param>
    /// <param name="builder">The WebApplicationBuilder instance</param>
    /// <returns>The configured web application builder.</returns>
    [Obsolete("Use extension for IServiceCollection")]
    public static WebApplicationBuilder AddDefaultAuthentication(this WebApplicationBuilder builder,
        ISettings settings)
    {
        builder.Services.AddDefaultAuthentication(settings);
        return builder;
    }

    private static string SelectDefaultAuthenticationScheme(HttpContext context)
    {
        var authorization = context.Request.Headers.Authorization.ToString();
        if (authorization.StartsWith($"{Bearer} ", StringComparison.OrdinalIgnoreCase))
            return JwtBearerDefaults.AuthenticationScheme;

        if (context.Request.Path.StartsWithSegments("/hubs") &&
            !string.IsNullOrEmpty(context.Request.Query["access_token"]))
            return JwtBearerDefaults.AuthenticationScheme;

        return CookieAuthenticationScheme;
    }

    private static IServiceCollection AddAuthenticationTicketStore(
        this IServiceCollection services,
        AuthTicketStoreSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.RedisConnectionString))
        {
            services.AddStackExchangeRedisCache(options => { options.Configuration = settings.RedisConnectionString; });
            services.AddSingleton<ITicketStore>(provider => new DistributedAuthenticationTicketStore(
                provider.GetRequiredService<IDistributedCache>(),
                provider.GetRequiredService<IDataProtectionProvider>(),
                provider.GetRequiredService<ILogger<DistributedAuthenticationTicketStore>>(),
                settings.DistributedKeyPrefix,
                new InMemoryAuthenticationTicketStore(
                    settings.MaxInMemoryTickets,
                    TimeSpan.FromSeconds(settings.CleanupIntervalSeconds))));
        }
        else
        {
            services.AddSingleton<ITicketStore>(_ => new InMemoryAuthenticationTicketStore(
                settings.MaxInMemoryTickets,
                TimeSpan.FromSeconds(settings.CleanupIntervalSeconds)));
        }

        services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>,
            CookieAuthenticationTicketStorePostConfigure>();

        return services;
    }

    private static OpenIdConnectConfiguration CreateOpenIdConnectConfiguration(string publicUrlWithRealm,
        string backchannelUrlWithRealm)
    {
        return new OpenIdConnectConfiguration
        {
            Issuer = publicUrlWithRealm,
            AuthorizationEndpoint = publicUrlWithRealm.UrlAppend("protocol/openid-connect/auth"),
            EndSessionEndpoint = publicUrlWithRealm.UrlAppend("protocol/openid-connect/logout"),
            TokenEndpoint = backchannelUrlWithRealm.UrlAppend("protocol/openid-connect/token"),
            UserInfoEndpoint = backchannelUrlWithRealm.UrlAppend("protocol/openid-connect/userinfo"),
            JwksUri = backchannelUrlWithRealm.UrlAppend("protocol/openid-connect/certs")
        };
    }

    private static IEnumerable<string> CreateValidIssuers(string publicUrlWithRealm, string? internalUrlWithRealm)
    {
        return new[] { publicUrlWithRealm, internalUrlWithRealm }
            .Where(x => x is not null)
            .Select(x => x!);
    }

    private static IEnumerable<SecurityKey> GetSigningKeys(string urlWithRealm, bool allowInvalidCertificate)
    {
        using var handler = allowInvalidCertificate ? CreateDevelopmentHttpClientHandler() : new HttpClientHandler();
        using var client = new HttpClient(handler);
        var jwksJson = client.GetStringAsync(urlWithRealm.UrlAppend("protocol/openid-connect/certs")).Result;
        return JsonWebKeySet.Create(jwksJson).GetSigningKeys();
    }

    private static HttpClientHandler CreateDevelopmentHttpClientHandler()
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    }

    private sealed class OipOpenIdConnectProtocolValidator : OpenIdConnectProtocolValidator
    {
        public OipOpenIdConnectProtocolValidator()
        {
            RequireState = false;
        }
    }

    public static WebApplication UseOipCsrfProtection(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            if (RequiresCsrfValidation(context))
            {
                var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
                try
                {
                    await antiforgery.ValidateRequestAsync(context);
                }
                catch (AntiforgeryValidationException)
                {
                    await WriteAuthenticationError(context, 403, "Forbidden", "A valid CSRF token is required.");
                    return;
                }
            }

            await next();
        });

        return app;
    }

    private static bool RequiresCsrfValidation(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method) &&
            !HttpMethods.IsPut(context.Request.Method) &&
            !HttpMethods.IsPatch(context.Request.Method) &&
            !HttpMethods.IsDelete(context.Request.Method))
            return false;

        if (!context.Request.Path.StartsWithSegments("/api"))
            return false;

        if (context.Request.Path.StartsWithSegments("/api/security/create-auth-session"))
            return false;

        return context.User.Identity?.IsAuthenticated == true;
    }

    private static async Task WriteAuthenticationError(HttpContext context, int statusCode, string title,
        string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var response = new ApiExceptionResponse(title, message, statusCode);
        await context.Response.WriteAsync(JsonConvert.SerializeObject(response, JsonSettings.Value));
    }

    /// <summary>
    /// Build Oip Module application
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="builder">The WebApplicationBuilder instance</param>
    /// <returns></returns>
    [Obsolete("Use particle method call")]
    public static WebApplication BuildApp(this WebApplicationBuilder builder, ISettings settings)
    {
        var app = builder.Build();
        app.UseOipForwardedHeaders();
        app.AddRequestLocalization();
        app.AddExceptionHandler();
        app.MapDefaultEndpoints();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseOipCsrfProtection();
        app.UseAuthorization();
        app.UseCors(options => options.AllowAnyOrigin());
        app.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");
        app.MapOpenApi(settings);
        app.MapFallbackToFile("index.html");

        return app;
    }

    /// <summary>
    /// Enables forwarded headers middleware.
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder</returns>
    public static WebApplication UseOipForwardedHeaders(this WebApplication app)
    {
        app.UseForwardedHeaders();
        return app;
    }

    private static bool TryParseKnownNetwork(string value, out Microsoft.AspNetCore.HttpOverrides.IPNetwork network)
    {
        network = null!;
        var parts = value.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2 ||
            !IPAddress.TryParse(parts[0], out var prefix) ||
            !int.TryParse(parts[1], out var prefixLength))
            return false;

        try
        {
            network = new Microsoft.AspNetCore.HttpOverrides.IPNetwork(prefix, prefixLength);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>
    /// Maps default health check endpoints for application monitoring
    /// </summary>
    /// <param name="app">The application builder</param>
    public static void MapDefaultEndpoints(this WebApplication app)
    {
        // All health checks must pass for app to be considered ready to accept traffic after starting
        app.MapGet("/health", async (HealthCheckService health) =>
            {
                var result = await health.CheckHealthAsync();
                return result.Status == HealthStatus.Healthy
                    ? Results.Ok(result)
                    : Results.StatusCode(503);
            })
            .WithGroupName("v1")
            .WithName("HealthCheck")
            .WithTags("v1")
            .WithOpenApi();

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        app.MapHealthChecks("/liveness", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });
    }

    /// <summary>
    /// Configures HTTP Strict Transport Security to help protect against man-in-the-middle attacks
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns></returns>
    public static WebApplication AddHsts(this WebApplication app)
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
    /// <param name="app">The application builder</param>
    /// <returns></returns>
    public static WebApplication AddRequestLocalization(this WebApplication app)
    {
        var localizeOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
        if (localizeOptions != null) app.UseRequestLocalization(localizeOptions.Value);
        return app;
    }

    /// <summary>
    /// Configures the application to handle exceptions and return a JSON response
    /// </summary>
    /// <param name="app">The application builder</param>
    public static WebApplication AddExceptionHandler(this WebApplication app)
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
                    var logger = app.Services
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("Oip.ExceptionHandler");
                    logger.LogError(error.Error, "Unhandled exception while processing {Path}.", context.Request.Path);

                    ApiExceptionResponse response;
                    if (error.Error is ApiException oipException)
                    {
                        context.Response.StatusCode = oipException.StatusCode;
                        response = new ApiExceptionResponse(oipException.Title, oipException.Message,
                            oipException.StatusCode,
                            app.Environment.IsDevelopment() ? oipException.StackTrace : null);
                    }
                    else
                    {
                        var ex = error.Error;
                        response = new ApiExceptionResponse("Unexpected error", ex.Message, 500,
                            app.Environment.IsDevelopment() ? ex.StackTrace : null);
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(response, JsonSettings.Value));
                }
            });
        });
        return app;
    }

    /// <summary>
    /// Configures and maps Open API endpoints for the application
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <param name="app">The application builder</param>
    public static void MapOpenApi(this WebApplication app, ISettings settings)
    {
        if (settings.OpenApi.All(x => !x.Publish))
            return;
        app.UseSwagger();
        app.MapGet("/swagger/oip-csrf.js", () => Results.Text("""
                                                              (function() {
                                                                  const originalFetch = window.fetch.bind(window);
                                                                  const unsafeMethods = new Set(["POST", "PUT", "PATCH", "DELETE"]);
                                                                  const csrfTokenUrl = "/api/security/get-auth-csrf-token";

                                                                  function getUrl(input) {
                                                                      return new URL(input instanceof Request ? input.url : input, window.location.origin);
                                                                  }

                                                                  function getMethod(input, init) {
                                                                      return ((init && init.method) || (input instanceof Request && input.method) || "GET").toUpperCase();
                                                                  }

                                                                  function requiresCsrf(input, init) {
                                                                      const method = getMethod(input, init);
                                                                      if (!unsafeMethods.has(method)) {
                                                                          return false;
                                                                      }

                                                                      const url = getUrl(input);
                                                                      return url.pathname.startsWith("/api") &&
                                                                          !url.pathname.includes("/api/security/create-auth-session") &&
                                                                          !url.pathname.includes(csrfTokenUrl);
                                                                  }

                                                                  function appendCsrfHeader(input, init, csrfToken) {
                                                                      if (!csrfToken || !csrfToken.token) {
                                                                          return originalFetch(input, init);
                                                                      }

                                                                      const nextInit = Object.assign({}, init);
                                                                      const headers = new Headers(
                                                                          nextInit.headers || (input instanceof Request ? input.headers : undefined)
                                                                      );

                                                                      headers.set(csrfToken.headerName || "X-CSRF-TOKEN", csrfToken.token);
                                                                      nextInit.headers = headers;

                                                                      return originalFetch(input, nextInit);
                                                                  }

                                                                  window.fetch = function(input, init) {
                                                                      if (!requiresCsrf(input, init)) {
                                                                          return originalFetch(input, init);
                                                                      }

                                                                      return originalFetch(csrfTokenUrl, {
                                                                          method: "GET",
                                                                          credentials: "same-origin",
                                                                          headers: { "Accept": "application/json" }
                                                                      })
                                                                          .then(function(response) {
                                                                              if (!response.ok) {
                                                                                  return originalFetch(input, init);
                                                                              }

                                                                              return response.json()
                                                                                  .then(function(csrfToken) {
                                                                                      return appendCsrfHeader(input, init, csrfToken);
                                                                                  });
                                                                          })
                                                                          .catch(function() {
                                                                              return originalFetch(input, init);
                                                                          });
                                                                  };
                                                              })();
                                                              """, "application/javascript"));
        app.UseSwaggerUI(swaggerUiOptions =>
        {
            swaggerUiOptions.EnableTryItOutByDefault();
            swaggerUiOptions.InjectJavascript("/swagger/oip-csrf.js");
            settings.OpenApi.ForEach(api =>
            {
                if (api.Publish)
                {
                    swaggerUiOptions.SwaggerEndpoint(api.Url, api.Name);
                }
            });
        });
    }

    /// <summary>
    /// Configures and maps Open API endpoints for the application
    /// </summary>
    /// <param name="settings">The application settings</param>
    /// <param name="app">The application builder</param>
    public static void MapOpenTelemetry(this WebApplication app, ISettings settings)
    {
        if (settings.OpenTelemetry.Enable)
            app.MapPrometheusScrapingEndpoint();
    }


    private static readonly Lazy<JsonSerializerSettings> JsonSettings = new(() => new JsonSerializerSettings
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

    /// <summary>
    /// Configures data protection services
    /// </summary>
    public static void AddDataProtection(this IServiceCollection services, ISettings baseSettings)
    {
        var settings = baseSettings.DataProtection;

        var dataProtectionBuilder = services.AddDataProtection()
            .SetApplicationName("OIP");
        if (settings.PersistKeysToFileSystemPath is not null)
            dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(settings.PersistKeysToFileSystemPath));
        dataProtectionBuilder.SetDefaultKeyLifetime(TimeSpan.FromDays(settings.DefaultKeyLifetimeInDay));

        services.AddScoped<CryptService>();
    }

    /// <summary>
    /// Adds the OIP CORS policy provider to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the service to.</param>
    /// <param name="baseSettings">The base OIP module application settings.</param>
    public static IServiceCollection AddCors(this IServiceCollection services, ISettings baseSettings)
    {
        services.AddCors()
            .AddSingleton<ICorsPolicyProvider>(new CorsPolicyProvider(baseSettings.Cors));
        return services;
    }

    /// <summary>
    /// Registers a controller type that should be exposed by ASP.NET Core MVC.
    /// </summary>
    public static IServiceCollection AddController<TController>(this IServiceCollection services)
        where TController : ControllerBase
    {
        GetOrCreateRegistry(services).Add(typeof(TController));
        return services;
    }

    internal static ExplicitControllerRegistry GetOrCreateRegistry(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ExplicitControllerRegistry));
        if (descriptor?.ImplementationInstance is ExplicitControllerRegistry existingRegistry)
            return existingRegistry;

        var newRegistry = new ExplicitControllerRegistry();

        var mvcBuilder = CommonMvcBuilderProducer(services);

        mvcBuilder.ConfigureApplicationPartManager(partManager =>
        {
            for (var i = partManager.FeatureProviders.Count - 1; i >= 0; i--)
            {
                if (partManager.FeatureProviders[i] is ControllerFeatureProvider)
                    partManager.FeatureProviders.RemoveAt(i);
            }

            partManager.FeatureProviders.Add(new ExplicitControllerFeatureProvider(newRegistry));
        });

        services.AddSingleton(newRegistry);
        return newRegistry;
    }

    internal static ExplicitControllerRegistry? GetExistingRegistry(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(x =>
            x.ServiceType == typeof(ExplicitControllerRegistry));

        return descriptor?.ImplementationInstance as ExplicitControllerRegistry;
    }
}