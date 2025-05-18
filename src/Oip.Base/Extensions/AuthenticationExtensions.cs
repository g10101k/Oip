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
    /// Add auth service
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
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
                };
            });
        builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformation>();
        builder.Services.AddAuthorization();
        return builder;
    }
}