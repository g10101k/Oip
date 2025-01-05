using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Oip.Base.Middlewares;
using Oip.Base.Settings;

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
                options.MetadataAddress =
                    $"{settings.SecurityService.BaseUrl}/realms/{settings.SecurityService.Realm}/.well-known/openid-configuration";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = $"{settings.SecurityService.BaseUrl}/realms/{settings.SecurityService.Realm}",
                    ValidateAudience = false,
                };
            });
        builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformation>();
        builder.Services.AddAuthorization();
        return builder;
    }
}