using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Oip.Security.IntegrationTests.Tests.Base;
using Oip.Security.UI.Configuration.Constants;
using Xunit;

namespace Oip.Security.IntegrationTests.Tests;

public class LogControllerTests : BaseClassFixture
{
    public LogControllerTests(TestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ReturnRedirectInErrorsLogWithoutAdminRole()
    {
        //Remove
        Client.DefaultRequestHeaders.Clear();

        // Act
        var response = await Client.GetAsync("/log/errorslog");

        // Assert           
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);

        //The redirect to login
        response.Headers.Location.ToString().Should().Contain(AuthenticationConsts.AccountLoginPage);
    }

    [Fact]
    public async Task ReturnRedirectInAuditLogWithoutAdminRole()
    {
        //Remove
        Client.DefaultRequestHeaders.Clear();

        // Act
        var response = await Client.GetAsync("/log/auditlog");

        // Assert           
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);

        //The redirect to login
        response.Headers.Location.ToString().Should().Contain(AuthenticationConsts.AccountLoginPage);
    }

    [Fact]
    public async Task ReturnSuccessInErrorsLogWithAdminRole()
    {
        //Remove
        Client.DefaultRequestHeaders.Clear();

        SetupAdminClaimsViaHeaders();

        // Act
        var response = await Client.GetAsync("/log/errorslog");

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReturnSuccessInAuditLogWithAdminRole()
    {
        SetupAdminClaimsViaHeaders();

        // Act
        var response = await Client.GetAsync("/log/auditlog");

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}