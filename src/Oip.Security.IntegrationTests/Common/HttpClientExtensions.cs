using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using Oip.Security.UI.Configuration;
using Oip.Security.UI.Middlewares;

namespace Oip.Security.IntegrationTests.Common;

public static class HttpClientExtensions
{
    public static void SetAdminClaimsViaHeaders(this HttpClient client, AdminConfiguration adminConfiguration)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, adminConfiguration.AdministrationRole)
        };

        var token = new JwtSecurityToken(claims: claims);
        var t = new JwtSecurityTokenHandler().WriteToken(token);
        client.DefaultRequestHeaders.Add(AuthenticatedTestRequestMiddleware.TestAuthorizationHeader, t);
    }
}