namespace Oip.Base.Exceptions;

/// <summary>
/// Represents the base exception class for Oip applications.
/// </summary>
/// <param name="message">Exception message.</param>
/// <param name="statusCode">HTTP status code associated with the exception.</param>
/// <param name="stackTrace">Stacktrace.</param>
public class OipException(
    string message,
    int statusCode = 500,
    string? stackTrace = null)
{
    /// <summary>
    /// Exception message.
    /// </summary>
    public string Message { get; } = message;

    /// <summary>
    /// The HTTP status code associated with the exception.
    /// </summary>
    public int StatusCode { get; set; } = statusCode;

    /// <summary>
    /// The stack trace for the exception.
    /// </summary>
    public string? StackTrace { get; } = stackTrace;
}