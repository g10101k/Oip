using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Oip.Base.Services;

namespace Oip.Test;

/// <summary>
/// Tests for <see cref="InMemoryAuthenticationTicketStore"/> session enumeration.
/// </summary>
[TestFixture]
public class InMemoryAuthenticationTicketStoreTests
{
    [Test]
    public async Task StoreAsync_CapturesSessionMetadataAndExposesIt()
    {
        var store = new InMemoryAuthenticationTicketStore();
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.0.0.7");
        context.Request.Headers.UserAgent = "UnitTest/1.0";

        var ticket = CreateTicket("user-1", "alice");
        var key = await store.StoreAsync(ticket, context, CancellationToken.None);

        var sessions = await store.GetSessionsAsync();

        Assert.That(sessions, Has.Count.EqualTo(1));
        var session = sessions[0];
        Assert.Multiple(() =>
        {
            Assert.That(session.Key, Is.EqualTo(key));
            Assert.That(session.SessionId, Is.EqualTo(AuthSessionMetadata.ComputeSessionId(key)));
            Assert.That(session.SessionId, Is.Not.EqualTo(key));
            Assert.That(session.UserId, Is.EqualTo("user-1"));
            Assert.That(session.UserName, Is.EqualTo("alice"));
            Assert.That(session.IpAddress, Is.EqualTo("10.0.0.7"));
            Assert.That(session.UserAgent, Is.EqualTo("UnitTest/1.0"));
            Assert.That(session.CreatedUtc, Is.Not.Null);
            Assert.That(session.LastActivityUtc, Is.Not.Null);
            Assert.That(ticket.Principal.FindFirstValue(AuthSessionMetadata.SessionKeyClaimType), Is.EqualTo(key));
        });
    }

    [Test]
    public async Task RemoveAsync_DropsSessionSoRetrieveReturnsNull()
    {
        var store = new InMemoryAuthenticationTicketStore();
        var key = await store.StoreAsync(CreateTicket("user-1", "alice"));

        await store.RemoveAsync(key);

        Assert.Multiple(async () =>
        {
            Assert.That(await store.RetrieveAsync(key), Is.Null);
            Assert.That(await store.GetSessionsAsync(), Is.Empty);
        });
    }

    [Test]
    public async Task GetSessionsAsync_SkipsExpiredTickets()
    {
        var store = new InMemoryAuthenticationTicketStore();
        await store.StoreAsync(CreateTicket("user-1", "alice", DateTimeOffset.UtcNow.AddMinutes(-1)));
        await store.StoreAsync(CreateTicket("user-2", "bob"));

        var sessions = await store.GetSessionsAsync();

        Assert.That(sessions.Select(x => x.UserId), Is.EquivalentTo(new[] { "user-2" }));
    }

    [Test]
    public void TouchActivity_UpdatesOnlyAfterInterval()
    {
        var ticket = CreateTicket("user-1", "alice");
        var now = DateTimeOffset.UtcNow;
        AuthSessionMetadata.Initialize("key", ticket, null, now);

        Assert.Multiple(() =>
        {
            Assert.That(AuthSessionMetadata.TouchActivity(ticket, now.AddSeconds(10)), Is.False);
            Assert.That(AuthSessionMetadata.TouchActivity(ticket, now.Add(AuthSessionMetadata.ActivityUpdateInterval)),
                Is.True);
            Assert.That(AuthSessionMetadata.Create("key", ticket).LastActivityUtc,
                Is.EqualTo(now.Add(AuthSessionMetadata.ActivityUpdateInterval)));
        });
    }

    private static AuthenticationTicket CreateTicket(string userId, string userName, DateTimeOffset? expiresUtc = null)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("preferred_username", userName)
        ], "Test");
        var properties = new AuthenticationProperties
        {
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = expiresUtc ?? DateTimeOffset.UtcNow.AddHours(1)
        };
        return new AuthenticationTicket(new ClaimsPrincipal(identity), properties, "Test");
    }
}
