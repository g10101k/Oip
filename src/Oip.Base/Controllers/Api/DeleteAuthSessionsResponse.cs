namespace Oip.Base.Controllers.Api;

/// <summary>
/// Result of a bulk session termination.
/// </summary>
public sealed class DeleteAuthSessionsResponse
{
    /// <summary>
    /// Number of sessions removed.
    /// </summary>
    public int DeletedCount { get; set; }
}
