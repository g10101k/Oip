using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Repositories;
using Oip.Base.Exceptions;
using Oip.Base.Services;

namespace Oip.Base.Security;

/// <summary>
/// Requires the current user to hold the specified right on the module instance the request targets.
/// </summary>
/// <remarks>
/// The module instance is always taken from the <see cref="SecurityConstants.ModuleInstanceIdHeader" /> header, so
/// an <c>id</c> route or query parameter of an endpoint never affects the check.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RightAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Creates the attribute.
    /// </summary>
    /// <param name="right">The required right, see <see cref="SecurityConstants" />.</param>
    public RightAttribute(string right) : base(typeof(ModuleInstanceRightFilter))
    {
        Arguments = [right];
    }
}

/// <summary>
/// Authorization filter behind <see cref="RightAttribute" />.
/// </summary>
public class ModuleInstanceRightFilter(
    string right,
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

        var moduleInstanceId = context.HttpContext.Request.GetModuleInstanceId();
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
/// Reads the module instance identifier a request targets.
/// </summary>
public static class ModuleInstanceRequestExtensions
{
    /// <summary>
    /// Reads the module instance identifier from the <see cref="SecurityConstants.ModuleInstanceIdHeader" /> header.
    /// </summary>
    /// <param name="request">The current request.</param>
    /// <returns>The module instance identifier, or <c>null</c> when the header is missing or malformed.</returns>
    public static int? GetModuleInstanceId(this HttpRequest request)
    {
        if (request.Headers.TryGetValue(SecurityConstants.ModuleInstanceIdHeader, out var headerValue) &&
            int.TryParse(headerValue.ToString(), out var moduleInstanceId))
            return moduleInstanceId;

        return null;
    }
}
