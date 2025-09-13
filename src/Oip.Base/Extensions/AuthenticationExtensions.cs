using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Oip.Base.Middlewares;
using Oip.Base.Settings;
using Oip.Base.Helpers;

namespace Oip.Base.Extensions;

/// <summary>
/// Authentication Extensions
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Configures default authentication using JWT Bearer scheme.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="settings">The base Oip module application settings.</param>
    /// <returns>The configured web application builder.</returns>
    public static WebApplicationBuilder AddDefaultAuthentication(this WebApplicationBuilder builder,
        IBaseOipModuleAppSettings settings)
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
}