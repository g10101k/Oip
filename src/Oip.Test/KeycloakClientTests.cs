using System.Net;
using Oip.Base.Clients;

namespace Oip.Test;

[TestFixture]
public class KeycloakClientTests
{
    [Test]
    public async Task Authentication_PreservesBaseAddressPath()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """
                {
                  "access_token": "token",
                  "expires_in": 300,
                  "refresh_expires_in": 0,
                  "token_type": "Bearer",
                  "not_before_policy": 0,
                  "scope": ""
                }
                """)
        });
        using var httpClient = new HttpClient(handler);
        httpClient.BaseAddress = new Uri("https://localhost:8443/keycloak");
        var keycloakClient = new KeycloakClient(httpClient);

        await keycloakClient.Authentication("client", "secret", "master", CancellationToken.None);

        Assert.That(handler.Requests[0].RequestUri?.ToString(),
            Is.EqualTo("https://localhost:8443/keycloak/realms/master/protocol/openid-connect/token"));
    }

    [Test]
    public async Task AdminRequests_PreserveBaseAddressPath()
    {
        var handler = new RecordingHandler(request =>
        {
            if (request.RequestUri?.AbsolutePath.EndsWith("/protocol/openid-connect/token") == true)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                          "access_token": "token",
                          "expires_in": 300,
                          "refresh_expires_in": 0,
                          "token_type": "Bearer",
                          "not_before_policy": 0,
                          "scope": ""
                        }
                        """)
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]")
            };
        });
        using var httpClient = new HttpClient(handler);
        httpClient.BaseAddress = new Uri("https://localhost:8443/keycloak");
        var keycloakClient = new KeycloakClient(httpClient);

        await keycloakClient.Authentication("client", "secret", "master", CancellationToken.None);
        await keycloakClient.GetRoles("master", CancellationToken.None);

        Assert.That(handler.Requests[1].RequestUri?.ToString(),
            Is.EqualTo("https://localhost:8443/keycloak/admin/realms/master/roles"));
    }

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(responseFactory(request));
        }
    }
}
