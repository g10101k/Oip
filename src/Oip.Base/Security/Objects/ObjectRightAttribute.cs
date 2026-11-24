using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Oip.Base.Exceptions;

namespace Oip.Base.Security.Objects;

/// <summary>
/// Requires the current user to hold the right on the object whose identifier the request carries. Combine it with
/// <see cref="RightAttribute" />: the module instance right opens the module, the object right opens the object.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ObjectRightAttribute : TypeFilterAttribute
{
    private readonly string _objectType;
    private readonly string _right;
    private string _routeKey = "id";

    /// <summary>
    /// Creates the attribute.
    /// </summary>
    /// <param name="objectType">Code of the object type.</param>
    /// <param name="right">Required right.</param>
    public ObjectRightAttribute(string objectType, string right) : base(typeof(ObjectRightFilter))
    {
        _objectType = objectType;
        _right = right;
        Arguments = [objectType, right, _routeKey];
    }

    /// <summary>
    /// Name of the route or query value with the object identifier, id by default.
    /// </summary>
    public string RouteKey
    {
        get => _routeKey;
        init
        {
            _routeKey = value;
            Arguments = [_objectType, _right, value];
        }
    }
}

/// <summary>
/// Authorization filter behind <see cref="ObjectRightAttribute" />.
/// </summary>
public sealed class ObjectRightFilter(string objectType, string right, string routeKey, IObjectAccessService access)
    : IAsyncAuthorizationFilter
{
    /// <inheritdoc />
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = Error("Unauthorized", "Authentication is required.", StatusCodes.Status401Unauthorized);
            return;
        }

        var raw = context.RouteData.Values.TryGetValue(routeKey, out var routeValue)
            ? routeValue?.ToString()
            : context.HttpContext.Request.Query[routeKey].ToString();

        if (!long.TryParse(raw, out var id))
        {
            context.Result = Error("Bad request", $"The '{routeKey}' value with the object identifier is missing.",
                StatusCodes.Status400BadRequest);
            return;
        }

        if (!await access.HasRightAsync(new ObjectRef(objectType, id), right, context.HttpContext.RequestAborted))
            context.Result = Error("Forbidden", $"The '{right}' right on {objectType}:{id} is required.",
                StatusCodes.Status403Forbidden);
    }

    private static ObjectResult Error(string title, string message, int statusCode) =>
        new(new ApiExceptionResponse(title, message, statusCode)) { StatusCode = statusCode };
}
