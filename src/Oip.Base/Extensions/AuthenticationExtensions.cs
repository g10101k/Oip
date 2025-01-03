using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Oip.Base.Middlewares;

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
    /// <returns></returns>
    public static WebApplicationBuilder AddDefaultAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MetadataAddress = "https://s-gbt-wsn-00010:8443/realms/oip/.well-known/openid-configuration";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = "https://s-gbt-wsn-00010:8443/realms/oip",
                    ValidateAudience = false,
                };
            });
        builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformation>();
        builder.Services.AddAuthorization();
        return builder;
    }
}
