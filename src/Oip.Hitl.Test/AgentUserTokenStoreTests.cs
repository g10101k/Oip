using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using Oip.Base.Security.ServiceAccount;
using Oip.Hitl.Services;

namespace Oip.Hitl.Test;

public class AgentUserTokenStoreTests
{
    private const string RunId = "agent-1";

    private FakeKeycloak _keycloak = null!;
    private MemoryStorage _storage = null!;
    private ManualTimeProvider _time = null!;
    private AgentUserTokenStore _store = null!;

    [SetUp]
    public void SetUp()
    {
        _keycloak = new FakeKeycloak();
        _storage = new MemoryStorage();
        _time = new ManualTimeProvider();
        var options = new ServiceAccountOptions
        {
            TokenEndpoint = "https://keycloak/realms/oip/protocol/openid-connect/token",
            ClientId = "oip-backend",
            ClientSecret = "secret"
        };
        _store = new AgentUserTokenStore(_storage, new KeycloakTokenClient(new HttpClientFactory(_keycloak), options),
            new EphemeralDataProtectionProvider(), _time, NullLogger<AgentUserTokenStore>.Instance);
    }

    [Test]
    public async Task Store_ExchangesUserTokenForRefreshTokenAndKeepsItProtected()
    {
        var stored = await _store.StoreAsync(RunId, "user-token", TimeSpan.FromMinutes(30), CancellationToken.None);

        Assert.That(stored, Is.True);
        var request = _keycloak.Requests.Single();
        Assert.That(request["grant_type"], Is.EqualTo("urn:ietf:params:oauth:grant-type:token-exchange"));
        Assert.That(request["subject_token"], Is.EqualTo("user-token"));
        Assert.That(request["requested_token_type"], Is.EqualTo("urn:ietf:params:oauth:token-type:refresh_token"));
        Assert.That(request["client_id"], Is.EqualTo("oip-backend"));
        Assert.That(_storage.Values[RunId].Value, Does.Not.Contain("refresh-1"), "the stored token is protected");
        Assert.That(_storage.Values[RunId].ExpiresAt, Is.EqualTo(_time.Now + TimeSpan.FromMinutes(30)));
    }

    [Test]
    public async Task GetAccessToken_RefreshesExpiredTokenAndNarrowsItToAudience()
    {
        await _store.StoreAsync(RunId, "user-token", TimeSpan.FromMinutes(30), CancellationToken.None);

        var fresh = await _store.GetAccessTokenAsync(RunId, null, CancellationToken.None);
        _time.Now += TimeSpan.FromMinutes(5);
        var refreshed = await _store.GetAccessTokenAsync(RunId, null, CancellationToken.None);
        var narrowed = await _store.GetAccessTokenAsync(RunId, "oip-reports", CancellationToken.None);

        Assert.That(fresh!.AccessToken, Is.EqualTo("access-1"), "a fresh token is not refreshed");
        Assert.That(refreshed!.AccessToken, Is.EqualTo("access-2"));
        Assert.That(_keycloak.Requests[1]["refresh_token"], Is.EqualTo("refresh-1"));
        Assert.That(narrowed!.AccessToken, Is.EqualTo("access-3"));
        Assert.That(_keycloak.Requests[2]["subject_token"], Is.EqualTo("access-2"));
        Assert.That(_keycloak.Requests[2]["audience"], Is.EqualTo("oip-reports"));

        // The refresh token rotated by the refresh is used next time.
        _time.Now += TimeSpan.FromMinutes(5);
        await _store.GetAccessTokenAsync(RunId, null, CancellationToken.None);
        Assert.That(_keycloak.Requests[3]["refresh_token"], Is.EqualTo("refresh-2"));
    }

    [Test]
    public async Task GetAccessToken_ReturnsNullWhenSessionHasEndedOrRunHasNoToken()
    {
        await _store.StoreAsync(RunId, "user-token", TimeSpan.FromMinutes(30), CancellationToken.None);
        _time.Now += TimeSpan.FromMinutes(5);
        _keycloak.Reject = true;

        Assert.That(await _store.GetAccessTokenAsync(RunId, null, CancellationToken.None), Is.Null);
        Assert.That(await _store.GetAccessTokenAsync("agent-2", null, CancellationToken.None), Is.Null);
    }

    [Test]
    public async Task Store_ReturnsFalseWhenKeycloakRefusesExchange()
    {
        _keycloak.Reject = true;

        var stored = await _store.StoreAsync(RunId, "user-token", TimeSpan.FromMinutes(30), CancellationToken.None);

        Assert.That(stored, Is.False);
        Assert.That(_storage.Values, Is.Empty);
    }

    /// <summary>
    /// Token endpoint that issues numbered tokens valid for 5 minutes.
    /// </summary>
    private sealed class FakeKeycloak : HttpMessageHandler
    {
        private int _issued;

        public bool Reject { get; set; }

        public List<Dictionary<string, string>> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var form = (await request.Content!.ReadAsStringAsync(cancellationToken)).Split('&')
                .Select(x => x.Split('='))
                .ToDictionary(x => Uri.UnescapeDataString(x[0]), x => Uri.UnescapeDataString(x[1]));
            Requests.Add(form);
            if (Reject)
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                    { Content = new StringContent("""{"error":"invalid_grant"}""") };

            var n = ++_issued;
            var narrowing = form.ContainsKey("audience");
            var token = new Dictionary<string, object> { ["access_token"] = $"access-{n}", ["expires_in"] = 300 };
            if (!narrowing) token["refresh_token"] = $"refresh-{n}";
            return new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent(JsonSerializer.Serialize(token)) };
        }
    }

    private sealed class HttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private sealed class MemoryStorage : IAgentUserTokenStorage
    {
        public Dictionary<string, (string Value, DateTimeOffset ExpiresAt)> Values { get; } = [];

        public Task<string?> GetAsync(string runId) =>
            Task.FromResult(Values.TryGetValue(runId, out var value) ? value.Value : null);

        public Task SetAsync(string runId, string value, DateTimeOffset expiresAt)
        {
            Values[runId] = (value, expiresAt);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string runId)
        {
            Values.Remove(runId);
            return Task.CompletedTask;
        }
    }

    private sealed class ManualTimeProvider : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 10, 4, 12, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => Now;
    }
}
