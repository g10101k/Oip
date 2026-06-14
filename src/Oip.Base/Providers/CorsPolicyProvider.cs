using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Oip.Base.Settings;

namespace Oip.Base.Providers;

internal sealed class CorsPolicyProvider(CorsSettings settings) : ICorsPolicyProvider
{
    public Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
    {
        var corsSettings = GetCorsSettings(context, policyName);
        return Task.FromResult(corsSettings is null ? null : BuildPolicy(corsSettings));
    }

    private CorsItemSettings? GetCorsSettings(HttpContext context, string? policyName)
    {
        if (!string.IsNullOrWhiteSpace(policyName))
        {
            return settings.FirstOrDefault(cors =>
                string.Equals(cors.PolicyName, policyName, StringComparison.OrdinalIgnoreCase));
        }

        var origin = context.Request.Headers.Origin.ToString();
        if (!string.IsNullOrWhiteSpace(origin))
        {
            return settings.FirstOrDefault(cors =>
                cors.AllowedOrigins.Any(allowedOrigin =>
                    string.Equals(allowedOrigin, origin, StringComparison.OrdinalIgnoreCase)));
        }

        return settings.FirstOrDefault();
    }

    private static CorsPolicy BuildPolicy(CorsItemSettings itemSettings)
    {
        var builder = new CorsPolicyBuilder()
            .WithOrigins(GetAllowedOrigins(itemSettings));

        if (itemSettings.AllowAnyHeader)
        {
            builder.AllowAnyHeader();
        }

        if (itemSettings.AllowAnyMethod)
        {
            builder.AllowAnyMethod();
        }

        if (itemSettings.AllowCredentials)
        {
            builder.AllowCredentials();
        }

        return builder.Build();
    }

    private static string[] GetAllowedOrigins(CorsItemSettings itemSettings)
    {
        return itemSettings.AllowedOrigins
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}