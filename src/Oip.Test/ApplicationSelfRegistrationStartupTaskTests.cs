using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Oip.Applications.Base;
using Oip.Applications.Base.Contracts;
using Oip.Applications.Base.StartupTasks;
using Oip.Base.Settings;

namespace Oip.Test;

[TestFixture]
public class ApplicationSelfRegistrationStartupTaskTests
{
    [Test]
    public void ExecuteAsync_RegistrationFails_DoesNotThrow()
    {
        var registryServiceMock = new Mock<IApplicationRegistryService>();
        registryServiceMock
            .Setup(x => x.RegisterApplicationAsync(It.IsAny<ApplicationRegistryItemDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Registration service is unavailable."));

        var task = new ApplicationSelfRegistrationStartupTask(
            registryServiceMock.Object,
            CreateSettings(),
            NullLogger<ApplicationSelfRegistrationStartupTask>.Instance);

        Assert.DoesNotThrowAsync(() => task.ExecuteAsync());
        registryServiceMock.Verify(
            x => x.RegisterApplicationAsync(It.Is<ApplicationRegistryItemDto>(application =>
                application.Code == "test-app"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void ExecuteAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var registryServiceMock = new Mock<IApplicationRegistryService>();
        registryServiceMock
            .Setup(x => x.RegisterApplicationAsync(It.IsAny<ApplicationRegistryItemDto>(), cancellationTokenSource.Token))
            .ThrowsAsync(new OperationCanceledException(cancellationTokenSource.Token));

        var task = new ApplicationSelfRegistrationStartupTask(
            registryServiceMock.Object,
            CreateSettings(),
            NullLogger<ApplicationSelfRegistrationStartupTask>.Instance);

        Assert.ThrowsAsync<OperationCanceledException>(() => task.ExecuteAsync(cancellationTokenSource.Token));
    }

    private static IBaseOipModuleAppSettings CreateSettings()
    {
        var settingsMock = new Mock<IBaseOipModuleAppSettings>();
        settingsMock.Setup(x => x.Application).Returns(new ApplicationSettings
        {
            Code = "test-app",
            DisplayName = "Test App",
            BaseUrl = "https://localhost:50001",
            ApiBaseUrl = "https://localhost:5001",
            Icon = "pi pi-test",
            Order = 10,
            Enabled = true
        });

        return settingsMock.Object;
    }
}
