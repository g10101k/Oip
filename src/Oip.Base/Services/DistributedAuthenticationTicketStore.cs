using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Oip.Base.Services;

/// <summary>
/// Stores cookie authentication tickets in a distributed cache.
/// </summary>
public sealed class DistributedAuthenticationTicketStore(
    IDistributedCache cache,
    IDataProtectionProvider dataProtectionProvider,
    ILogger<DistributedAuthenticationTicketStore> logger,
    string keyPrefix = "Oip:AuthTicket:",
    InMemoryAuthenticationTicketStore? fallbackStore = null,
    string? redisConnectionString = null)
    : IAuthSessionStore, IDisposable
{
    private const string DataProtectionPurpose = "Oip.Base.Services.DistributedAuthenticationTicketStore.v1";

    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(DataProtectionPurpose);
    private readonly string _keyPrefix = string.IsNullOrWhiteSpace(keyPrefix) ? "Oip:AuthTicket:" : keyPrefix;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnectionMultiplexer? _connection;

    public Task<string> StoreAsync(AuthenticationTicket ticket) => StoreAsync(ticket, null);

    public Task<string> StoreAsync(AuthenticationTicket ticket, HttpContext httpContext,
        CancellationToken cancellationToken) => StoreAsync(ticket, (HttpContext?)httpContext);

    private async Task<string> StoreAsync(AuthenticationTicket ticket, HttpContext? httpContext)
    {
        var key = Guid.NewGuid().ToString("N");
        AuthSessionMetadata.Initialize(key, ticket, httpContext, DateTimeOffset.UtcNow);
        try
        {
            await StoreTicketAsync(key, ticket);
        }
        catch (Exception exception)
        {
            if (fallbackStore is null)
                throw;

            logger.LogWarning(exception,
                "Failed to store authentication ticket {TicketKey} in distributed cache. Using in-memory fallback.",
                key);
            await fallbackStore.RenewAsync(key, ticket);
        }

        return key;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        try
        {
            await StoreTicketAsync(key, ticket);
        }
        catch (Exception exception)
        {
            if (fallbackStore is null)
                throw;

            logger.LogWarning(exception,
                "Failed to renew authentication ticket {TicketKey} in distributed cache. Using in-memory fallback.",
                key);
            await fallbackStore.RenewAsync(key, ticket);
        }
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var cacheKey = GetCacheKey(key);
        byte[]? protectedPayload;

        try
        {
            protectedPayload = await cache.GetAsync(cacheKey);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to retrieve authentication ticket {TicketKey} from distributed cache.", key);
            return fallbackStore is null ? null : await fallbackStore.RetrieveAsync(key);
        }

        if (protectedPayload is null)
            return null;

        AuthenticationTicket? ticket;
        try
        {
            var payload = _protector.Unprotect(protectedPayload);
            ticket = TicketSerializer.Default.Deserialize(payload);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Authentication ticket {TicketKey} is corrupt and will be removed.", key);
            await RemoveAsync(key);
            return null;
        }

        if (ticket?.Properties.ExpiresUtc is not { } expiresUtc || expiresUtc <= DateTimeOffset.UtcNow)
        {
            await RemoveAsync(key);
            return null;
        }

        if (AuthSessionMetadata.TouchActivity(ticket, DateTimeOffset.UtcNow))
        {
            try
            {
                await StoreTicketAsync(key, ticket);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception,
                    "Failed to update last activity of authentication ticket {TicketKey} in distributed cache.", key);
            }
        }

        return ticket;
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await cache.RemoveAsync(GetCacheKey(key));
        }
        catch (Exception exception)
        {
            if (fallbackStore is null)
                throw;

            logger.LogWarning(exception,
                "Failed to remove authentication ticket {TicketKey} from distributed cache. Removing from in-memory fallback.",
                key);
        }

        if (fallbackStore is not null)
            await fallbackStore.RemoveAsync(key);
    }

    public async Task<IReadOnlyList<AuthSessionInfo>> GetSessionsAsync(CancellationToken cancellationToken = default)
    {
        var sessions = new Dictionary<string, AuthSessionInfo>(StringComparer.Ordinal);

        if (fallbackStore is not null)
        {
            foreach (var session in await fallbackStore.GetSessionsAsync(cancellationToken))
                sessions[session.Key] = session;
        }

        try
        {
            await foreach (var key in ScanKeysAsync(cancellationToken))
            {
                if (await RetrieveStoredTicketAsync(key) is { } ticket)
                    sessions[key] = AuthSessionMetadata.Create(key, ticket);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Failed to enumerate authentication tickets in distributed cache.");
        }

        return sessions.Values.ToList();
    }

    private async IAsyncEnumerable<string> ScanKeysAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync();
        var database = connection.GetDatabase();
        var pattern = _keyPrefix + "*";

        foreach (var endpoint in connection.GetEndPoints())
        {
            var server = connection.GetServer(endpoint);
            if (!server.IsConnected || server.IsReplica)
                continue;

            await foreach (var redisKey in server.KeysAsync(database.Database, pattern)
                               .WithCancellation(cancellationToken))
            {
                var cacheKey = (string?)redisKey;
                if (cacheKey is not null && cacheKey.Length > _keyPrefix.Length)
                    yield return cacheKey[_keyPrefix.Length..];
            }
        }
    }

    private async Task<IConnectionMultiplexer> GetConnectionAsync()
    {
        if (_connection is { IsConnected: true })
            return _connection;

        if (string.IsNullOrWhiteSpace(redisConnectionString))
            throw new InvalidOperationException("Redis connection string is not configured.");

        await _connectionLock.WaitAsync();
        try
        {
            if (_connection is { IsConnected: true })
                return _connection;

            _connection?.Dispose();
            _connection = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);
            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task<AuthenticationTicket?> RetrieveStoredTicketAsync(string key)
    {
        try
        {
            var protectedPayload = await cache.GetAsync(GetCacheKey(key));
            if (protectedPayload is null)
                return null;

            var ticket = TicketSerializer.Default.Deserialize(_protector.Unprotect(protectedPayload));
            return ticket?.Properties.ExpiresUtc is { } expiresUtc && expiresUtc > DateTimeOffset.UtcNow
                ? ticket
                : null;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to read authentication ticket {TicketKey} from distributed cache.", key);
            return null;
        }
    }

    private async Task StoreTicketAsync(string key, AuthenticationTicket ticket)
    {
        var cacheKey = GetCacheKey(key);
        if (ticket.Properties.ExpiresUtc is not { } expiresUtc || expiresUtc <= DateTimeOffset.UtcNow)
        {
            await cache.RemoveAsync(cacheKey);
            return;
        }

        var payload = TicketSerializer.Default.Serialize(ticket);
        var protectedPayload = _protector.Protect(payload);
        await cache.SetAsync(cacheKey, protectedPayload, new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = expiresUtc
        });
    }

    private string GetCacheKey(string key) => _keyPrefix + key;

    public void Dispose()
    {
        _connection?.Dispose();
        _connectionLock.Dispose();
    }
}
