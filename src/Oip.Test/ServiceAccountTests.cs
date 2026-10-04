using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Oip.Base.Extensions;
using Oip.Base.Security.ServiceAccount;
using Oip.Base.Settings;

namespace Oip.Test;

[TestFixture]
public class ServiceAccountPolicyTests
{
    private const string ClientId = "oip-backend";

    [Test]
    public async Task ServiceAccountToken_IsAllowed()
    {
        var user = CreateBearerUser(("azp", ClientId), ("client_id", ClientId));

        Assert.That(await AuthorizeAsync(user), Is.True);
    }

    [Test]
    public async Task UserTokenOfSameClient_IsRejected()
    {
        var user = CreateBearerUser(("azp", ClientId), ("preferred_username", "admin"));

        Assert.That(await AuthorizeAsync(user), Is.False);
    }

    [Test]
    public async Task ServiceAccountTokenOfOtherClient_IsRejected()
    {
        var user = CreateBearerUser(("azp", "other-client"), ("client_id", "other-client"));

        Assert.That(await AuthorizeAsync(user), Is.False);
    }

    [Test]
    public async Task AnonymousUser_IsRejected()
    {
        Assert.That(await AuthorizeAsync(new ClaimsPrincipal(new ClaimsIdentity())), Is.False);
    }

    [Test]
    public async Task NotConfiguredClient_RejectsEveryone()
    {
        var user = CreateBearerUser(("azp", ClientId), ("client_id", ClientId));

        Assert.That(await AuthorizeAsync(user, clientId: null), Is.False);
    }

    [Test]
    public void Policy_UsesOnlyJwtBearerScheme()
    {
        var policy = OipModuleApplication.CreateServiceAccountPolicy(ClientId);

        Assert.That(policy.AuthenticationSchemes, Is.EqualTo(new[] { JwtBearerDefaults.AuthenticationScheme }));
    }

    private static ClaimsPrincipal CreateBearerUser(params (string Type, string Value)[] claims)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            claims.Select(x => new Claim(x.Type, x.Value)),
            JwtBearerDefaults.AuthenticationScheme));
    }

    private static async Task<bool> AuthorizeAsync(ClaimsPrincipal user, string clientId = ClientId)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(options =>
            options.AddPolicy(OipModuleApplication.ServiceAccountPolicy,
                OipModuleApplication.CreateServiceAccountPolicy(clientId)));
        await using var provider = services.BuildServiceProvider();
        var authorizationService = provider.GetRequiredService<IAuthorizationService>();

        var result = await authorizationService.AuthorizeAsync(user, OipModuleApplication.ServiceAccountPolicy);
        return result.Succeeded;
    }
}

[TestFixture]
public class ServiceAccountTokenProviderTests
{
    private static readonly ServiceAccountOptions Options = new()
    {
        TokenEndpoint = "https://localhost:8443/realms/oip/protocol/openid-connect/token",
        ClientId = "oip-backend",
        ClientSecret = "secret"
    };

    [Test]
    public async Task GetAccessToken_RequestsClientCredentialsToken()
    {
        var handler = new TokenEndpointHandler(expiresIn: 300);
        using var provider = CreateProvider(handler, new ManualTimeProvider());

        var token = await provider.GetAccessTokenAsync();

        Assert.Multiple(() =>
        {
            Assert.That(token, Is.EqualTo("token-1"));
            Assert.That(handler.Requests, Has.Count.EqualTo(1));
            Assert.That(handler.Requests[0].Uri, Is.EqualTo(Options.TokenEndpoint));
            Assert.That(handler.Requests[0].Body,
                Is.EqualTo("grant_type=client_credentials&client_id=oip-backend&client_secret=secret"));
        });
    }

    [Test]
    public async Task GetAccessToken_ReusesTokenUntilRenewal()
    {
        var handler = new TokenEndpointHandler(expiresIn: 300);
        var time = new ManualTimeProvider();
        using var provider = CreateProvider(handler, time);

        await provider.GetAccessTokenAsync();
        time.Advance(TimeSpan.FromSeconds(260));
        var cached = await provider.GetAccessTokenAsync();
        time.Advance(TimeSpan.FromSeconds(20));
        var renewed = await provider.GetAccessTokenAsync();

        Assert.Multiple(() =>
        {
            Assert.That(cached, Is.EqualTo("token-1"));
            Assert.That(renewed, Is.EqualTo("token-2"));
            Assert.That(handler.Requests, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task GetAccessToken_ConcurrentCallsRequestSingleToken()
    {
        var handler = new TokenEndpointHandler(expiresIn: 300);
        using var provider = CreateProvider(handler, new ManualTimeProvider());

        var tokens = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => provider.GetAccessTokenAsync()));

        Assert.Multiple(() =>
        {
            Assert.That(tokens, Is.All.EqualTo("token-1"));
            Assert.That(handler.Requests, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Invalidate_ForcesNewToken()
    {
        var handler = new TokenEndpointHandler(expiresIn: 300);
        using var provider = CreateProvider(handler, new ManualTimeProvider());

        var first = await provider.GetAccessTokenAsync();
        provider.Invalidate("stale-token");
        var stillCached = await provider.GetAccessTokenAsync();
        provider.Invalidate(first);
        var renewed = await provider.GetAccessTokenAsync();

        Assert.Multiple(() =>
        {
            Assert.That(stillCached, Is.EqualTo("token-1"));
            Assert.That(renewed, Is.EqualTo("token-2"));
        });
    }

    [Test]
    public void GetAccessToken_ThrowsWhenTokenEndpointFails()
    {
        var handler = new TokenEndpointHandler(expiresIn: 300, statusCode: HttpStatusCode.Unauthorized);
        using var provider = CreateProvider(handler, new ManualTimeProvider());

        Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetAccessTokenAsync());
    }

    [Test]
    public void FromSecurityService_PrefersDockerUrl()
    {
        var settings = new SecurityServiceSettings
        {
            BaseUrl = "https://localhost:8443/",
            DockerUrl = "https://keycloak:8443/",
            Realm = "oip",
            ClientId = "oip-backend",
            ClientSecret = "secret"
        };

        var options = ServiceAccountOptions.FromSecurityService(settings, isDevelopment: true);

        Assert.Multiple(() =>
        {
            Assert.That(options.TokenEndpoint,
                Is.EqualTo("https://keycloak:8443/realms/oip/protocol/openid-connect/token"));
            Assert.That(options.ClientId, Is.EqualTo("oip-backend"));
            Assert.That(options.ClientSecret, Is.EqualTo("secret"));
            Assert.That(options.AcceptAnyServerCertificate, Is.True);
        });
    }

    private static ServiceAccountTokenProvider CreateProvider(TokenEndpointHandler handler, TimeProvider time)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(x => x.CreateClient(ServiceAccountTokenProvider.HttpClientName))
            .Returns(() => new HttpClient(handler, disposeHandler: false));
        return new ServiceAccountTokenProvider(factory.Object, Options, time);
    }
}

[TestFixture]
public class ServiceAccountAuthorizationHandlerTests
{
    [Test]
    public async Task SendAsync_AddsBearerToken()
    {
        var tokenProvider = new Mock<IServiceAccountTokenProvider>();
        tokenProvider.Setup(x => x.GetAccessTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync("token");
        HttpRequestMessage sent = null;
        using var invoker = CreateInvoker(tokenProvider.Object, request =>
        {
            sent = request;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Post, "https://localhost/grpc"),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(sent.Headers.Authorization?.Scheme, Is.EqualTo("Bearer"));
            Assert.That(sent.Headers.Authorization?.Parameter, Is.EqualTo("token"));
        });
        tokenProvider.Verify(x => x.Invalidate(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SendAsync_InvalidatesTokenOnUnauthorized()
    {
        var tokenProvider = new Mock<IServiceAccountTokenProvider>();
        tokenProvider.Setup(x => x.GetAccessTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync("token");
        using var invoker = CreateInvoker(tokenProvider.Object,
            _ => new HttpResponseMessage(HttpStatusCode.Unauthorized));

        await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Post, "https://localhost/grpc"),
            CancellationToken.None);

        tokenProvider.Verify(x => x.Invalidate("token"), Times.Once);
    }

    private static HttpMessageInvoker CreateInvoker(IServiceAccountTokenProvider tokenProvider,
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        return new HttpMessageInvoker(new ServiceAccountAuthorizationHandler(tokenProvider)
        {
            InnerHandler = new StubHandler(responseFactory)
        });
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(responseFactory(request));
        }
    }
}

internal sealed class ManualTimeProvider : TimeProvider
{
    private DateTimeOffset _now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan delta) => _now += delta;
}

internal sealed class TokenEndpointHandler(int expiresIn, HttpStatusCode statusCode = HttpStatusCode.OK)
    : HttpMessageHandler
{
    private int _issued;

    public List<(string Uri, string Body)> Requests { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var body = request.Content is null ? "" : await request.Content.ReadAsStringAsync(cancellationToken);
        lock (Requests)
            Requests.Add((request.RequestUri!.ToString(), body));

        if (statusCode != HttpStatusCode.OK)
            return new HttpResponseMessage(statusCode) { Content = new StringContent("""{"error":"unauthorized_client"}""") };

        var token = $"token-{Interlocked.Increment(ref _issued)}";
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                $$"""{"access_token":"{{token}}","expires_in":{{expiresIn}},"token_type":"Bearer"}""",
                System.Text.Encoding.UTF8, "application/json")
        };
    }
}
