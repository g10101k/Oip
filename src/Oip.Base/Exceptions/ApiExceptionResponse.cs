namespace Oip.Base.Exceptions;

/// <summary>
/// Standardized response format for API exceptions
/// </summary>
/// <param name="Title">User-friendly title of the exception</param>
/// <param name="Message">Detailed description of the error</param>
/// <param name="StatusCode">HTTP status code associated with the exception</param>
/// <param name="StackTrace">Stack trace information (optional, typically omitted in production)</param>
public record ApiExceptionResponse(
    string Title,
    string Message,
    int StatusCode = 500,
    string? StackTrace = null);

/// <summary>Representation of a custom exception type for Oip applications with title and status code support</summary>
/// <param name="title">The title of the exception</param>
/// <param name="statusCode">The HTTP status code associated with the exception</param>
/// <param name="message">The detailed exception message</param>
public class ApiException(string title, string? message = null, int statusCode = 500) : Exception(message)
{
    /// <summary>
    /// The title of the exception
    /// </summary>
    public string Title { get; } = title;

    /// <summary>
    /// The HTTP status code associated with the exception
    /// </summary>
    public int StatusCode { get; } = statusCode;
}