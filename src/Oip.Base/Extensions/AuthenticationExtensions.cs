using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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
            .AddJwtBearer(options =>
            {
                var urlWithRealm = settings.SecurityService.BaseUrl
                    .UrlAppend("realms")
                    .UrlAppend(settings.SecurityService.Realm);

                options.MetadataAddress = urlWithRealm.UrlAppend(".well-known/openid-configuration");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = urlWithRealm,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromSeconds(settings.SecurityService.ClockSkewSeconds),
                };
            });
        builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformation>();
        builder.Services.AddAuthorization();
        return builder;
    }
}