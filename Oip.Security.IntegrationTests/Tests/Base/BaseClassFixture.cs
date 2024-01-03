using System.Net.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Oip.Security.IntegrationTests.Common;
using Oip.Security.UI.Configuration;
using Xunit;

namespace Oip.Security.IntegrationTests.Tests.Base;

public class BaseClassFixture : IClassFixture<TestFixture>
{
    protected readonly HttpClient Client;
    protected readonly TestServer TestServer;

    public BaseClassFixture(TestFixture fixture)
    {
        Client = fixture.Client;
        TestServer = fixture.TestServer;
    }

    protected virtual void SetupAdminClaimsViaHeaders()
    {
        using (var scope = TestServer.Services.CreateScope())
        {
            var configuration = scope.ServiceProvider.GetRequiredService<AdminConfiguration>();
            Client.SetAdminClaimsViaHeaders(configuration);
        }
    }
}