using System.Net;
using System.Net.Http.Headers;

namespace Oip.Base.Security.ServiceAccount;

/// <summary>
/// Adds the service account access token to outgoing requests as an <c>Authorization: Bearer</c> header.
/// </summary>
public sealed class ServiceAccountAuthorizationHandler(IServiceAccountTokenProvider tokenProvider)
    : DelegatingHandler
{
    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await tokenProvider.GetAccessTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            tokenProvider.Invalidate(accessToken);

        return response;
    }
}
