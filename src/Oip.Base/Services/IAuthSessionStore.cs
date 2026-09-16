using Microsoft.AspNetCore.Authentication.Cookies;

namespace Oip.Base.Services;

/// <summary>
/// A ticket store that can enumerate the sessions it holds.
/// </summary>
public interface IAuthSessionStore : ITicketStore
{
    /// <summary>
    /// Returns every active session known to the store.
    /// </summary>
    Task<IReadOnlyList<AuthSessionInfo>> GetSessionsAsync(CancellationToken cancellationToken = default);
}
