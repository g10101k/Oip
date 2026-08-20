using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Repositories;
using Oip.Base.Exceptions;
using Oip.Base.Services;

namespace Oip.Base.Security;

/// <summary>
/// Requires the current user to hold the specified right on the module instance the request targets.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RightAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Creates the attribute.
    /// </summary>
    /// <param name="right">The required right, see <see cref="SecurityConstants" />.</param>
    /// <param name="source">
    /// Where to read the module instance identifier from. Use <see cref="ModuleInstanceIdSource.Header" /> on endpoints
    /// whose <c>id</c> route or query parameter means something else, such as <c>update/{id}</c> of a domain entity.
    /// </param>
    public RightAttribute(string right,
        ModuleInstanceIdSource source = ModuleInstanceIdSource.Auto) : base(typeof(ModuleInstanceRightFilter))
    {
        Arguments = [right, source];
    }
}

/// <summary>
/// Describes where <see cref="RightAttribute" /> reads the module instance identifier from.
/// </summary>
public enum ModuleInstanceIdSource
{
    /// <summary>
    /// Route values and query string first, then the <see cref="SecurityConstants.ModuleInstanceIdHeader" /> header.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// The <see cref="SecurityConstants.ModuleInstanceIdHeader" /> header only.
    /// </summary>
    Header = 1
}

/// <summary>
/// Authorization filter behind <see cref="RightAttribute" />.
/// </summary>
public class ModuleInstanceRightFilter(
    string right,
    ModuleInstanceIdSource source,
    ModuleRepository moduleRepository,
    ClaimService claimService)
    : IAsyncAuthorizationFilter
{
    /// <inheritdoc />
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = Forbid("Authentication is required to access this module instance.");
            return;
        }

        var moduleInstanceId = ModuleInstanceIdResolver.Resolve(context.HttpContext.Request, context.RouteData, source);
        if (moduleInstanceId is null)
        {
            context.Result = Forbid("Module instance is not specified for this request.");
            return;
        }

        var hasRight = await moduleRepository.HasInstanceRight(moduleInstanceId.Value, claimService.GetUserRoles(),
            right);
        if (!hasRight)
        {
            context.Result =
                Forbid($"The '{right}' right on module instance {moduleInstanceId.Value} is required.");
        }
    }

    private static ObjectResult Forbid(string message)
    {
        return new ObjectResult(new ApiExceptionResponse("Forbidden", message, StatusCodes.Status403Forbidden))
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }
}

/// <summary>
/// Resolves the module instance identifier a request targets.
/// </summary>
public static class ModuleInstanceIdResolver
{
    private static readonly string[] IdParameterNames = ["id", "moduleInstanceId"];

    /// <summary>
    /// Resolves the module instance identifier from the request.
    /// </summary>
    /// <param name="request">The current request.</param>
    /// <param name="routeData">The route data of the current request.</param>
    /// <param name="source">Where to read the identifier from.</param>
    /// <returns>The module instance identifier, or <c>null</c> when it cannot be resolved.</returns>
    public static int? Resolve(HttpRequest request, RouteData routeData,
        ModuleInstanceIdSource source = ModuleInstanceIdSource.Auto)
    {
        if (source == ModuleInstanceIdSource.Auto)
        {
            foreach (var name in IdParameterNames)
            {
                if (routeData.Values.TryGetValue(name, out var routeValue) &&
                    int.TryParse(routeValue?.ToString(), out var fromRoute))
                    return fromRoute;

                if (request.Query.TryGetValue(name, out var queryValue) &&
                    int.TryParse(queryValue.ToString(), out var fromQuery))
                    return fromQuery;
            }
        }

        if (request.Headers.TryGetValue(SecurityConstants.ModuleInstanceIdHeader, out var headerValue) &&
            int.TryParse(headerValue.ToString(), out var fromHeader))
            return fromHeader;

        return null;
    }
}
