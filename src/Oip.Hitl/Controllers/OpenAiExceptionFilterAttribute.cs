using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Oip.Base.Exceptions;
using Oip.Hitl.Controllers.Api;

namespace Oip.Hitl.Controllers;

/// <summary>
/// Returns the errors of the OpenAI-compatible API in the OpenAI format, which chat UIs show to the user, instead
/// of <see cref="ApiExceptionResponse"/>.
/// </summary>
public sealed class OpenAiExceptionFilterAttribute : ExceptionFilterAttribute
{
    /// <inheritdoc />
    public override void OnException(ExceptionContext context)
    {
        if (context.HttpContext.Response.HasStarted) return;

        var (statusCode, error) = ToError(context.Exception);
        if (statusCode >= StatusCodes.Status500InternalServerError)
            context.HttpContext.RequestServices.GetRequiredService<ILogger<OpenAiExceptionFilterAttribute>>()
                .LogError(context.Exception, "OpenAI-compatible API request failed");

        context.Result = new JsonResult(new OpenAiErrorResponse(error), OpenAiJson.Options) { StatusCode = statusCode };
        context.ExceptionHandled = true;
    }

    /// <summary>
    /// Converts an exception to an OpenAI error; only the message of an <see cref="ApiException"/> is shown.
    /// </summary>
    public static (int StatusCode, OpenAiError Error) ToError(Exception exception)
    {
        if (exception is not ApiException apiException)
            return (StatusCodes.Status500InternalServerError,
                new OpenAiError("Internal server error", "server_error"));

        var type = apiException.StatusCode switch
        {
            StatusCodes.Status401Unauthorized => "authentication_error",
            StatusCodes.Status403Forbidden => "permission_error",
            StatusCodes.Status404NotFound => "invalid_request_error",
            < 500 => "invalid_request_error",
            _ => "server_error"
        };
        return (apiException.StatusCode, new OpenAiError(apiException.Message, type));
    }
}
