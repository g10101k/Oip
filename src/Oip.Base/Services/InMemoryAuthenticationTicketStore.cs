using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Oip.Base.Services;

/// <summary>
/// Stores cookie authentication tickets server-side so the browser receives only an opaque session key.
/// </summary>
public sealed class InMemoryAuthenticationTicketStore : ITicketStore
{
    private readonly ConcurrentDictionary<string, StoredAuthenticationTicket> _tickets = new();
    private readonly TimeSpan _cleanupInterval;
    private readonly object _cleanupLock = new();
    private DateTimeOffset _nextCleanupUtc = DateTimeOffset.UtcNow;

    public InMemoryAuthenticationTicketStore(int maxTickets = 10000, TimeSpan? cleanupInterval = null)
    {
        MaxTickets = Math.Max(1, maxTickets);
        _cleanupInterval = cleanupInterval.GetValueOrDefault(TimeSpan.FromSeconds(300));
    }

    private int MaxTickets { get; }

    public Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString("N");
        _tickets[key] = new StoredAuthenticationTicket(ticket, DateTimeOffset.UtcNow);
        CleanupIfNeeded(forceLimitCheck: true);
        return Task.FromResult(key);
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        _tickets[key] = new StoredAuthenticationTicket(ticket, DateTimeOffset.UtcNow);
        CleanupIfNeeded(forceLimitCheck: true);
        return Task.CompletedTask;
    }

    public Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        CleanupIfNeeded(forceLimitCheck: false);

        if (!_tickets.TryGetValue(key, out var storedTicket))
            return Task.FromResult<AuthenticationTicket?>(null);

        if (IsExpired(storedTicket.Ticket, DateTimeOffset.UtcNow))
        {
            _tickets.TryRemove(key, out _);
            return Task.FromResult<AuthenticationTicket?>(null);
        }

        return Task.FromResult<AuthenticationTicket?>(storedTicket.Ticket);
    }

    public Task RemoveAsync(string key)
    {
        _tickets.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    private void CleanupIfNeeded(bool forceLimitCheck)
    {
        var now = DateTimeOffset.UtcNow;
        if (!forceLimitCheck && now < _nextCleanupUtc)
            return;

        if (forceLimitCheck && _tickets.Count <= MaxTickets && now < _nextCleanupUtc)
            return;

        lock (_cleanupLock)
        {
            now = DateTimeOffset.UtcNow;
            if (!forceLimitCheck && now < _nextCleanupUtc)
                return;

            RemoveExpired(now);
            TrimOldestTickets();
            _nextCleanupUtc = now.Add(_cleanupInterval);
        }
    }

    private void RemoveExpired(DateTimeOffset now)
    {
        foreach (var pair in _tickets)
        {
            if (IsExpired(pair.Value.Ticket, now))
                _tickets.TryRemove(pair.Key, out _);
        }
    }

    private void TrimOldestTickets()
    {
        var overflow = _tickets.Count - MaxTickets;
        if (overflow <= 0)
            return;

        foreach (var pair in _tickets
                     .OrderBy(x => x.Value.StoredUtc)
                     .Take(overflow))
        {
            _tickets.TryRemove(pair.Key, out _);
        }
    }

    private static bool IsExpired(AuthenticationTicket ticket, DateTimeOffset now) =>
        ticket.Properties.ExpiresUtc is { } expiresUtc && expiresUtc <= now;

    private sealed record StoredAuthenticationTicket(AuthenticationTicket Ticket, DateTimeOffset StoredUtc);
}
