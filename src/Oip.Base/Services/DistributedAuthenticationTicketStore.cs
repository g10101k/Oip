using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Oip.Base.Services;

/// <summary>
/// Stores cookie authentication tickets in a distributed cache.
/// </summary>
public sealed class DistributedAuthenticationTicketStore(
    IDistributedCache cache,
    IDataProtectionProvider dataProtectionProvider,
    ILogger<DistributedAuthenticationTicketStore> logger,
    string keyPrefix = "Oip:AuthTicket:",
    InMemoryAuthenticationTicketStore? fallbackStore = null)
    : ITicketStore
{
    private const string DataProtectionPurpose = "Oip.Base.Services.DistributedAuthenticationTicketStore.v1";

    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(DataProtectionPurpose);
    private readonly string _keyPrefix = string.IsNullOrWhiteSpace(keyPrefix) ? "Oip:AuthTicket:" : keyPrefix;

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = Guid.NewGuid().ToString("N");
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

        try
        {
            var payload = _protector.Unprotect(protectedPayload);
            var ticket = TicketSerializer.Default.Deserialize(payload);

            if (ticket?.Properties.ExpiresUtc is not { } expiresUtc || expiresUtc <= DateTimeOffset.UtcNow)
            {
                await RemoveAsync(key);
                return null;
            }

            return ticket;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Authentication ticket {TicketKey} is corrupt and will be removed.", key);
            await RemoveAsync(key);
            return null;
        }
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
}
