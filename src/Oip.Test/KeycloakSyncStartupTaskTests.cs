using Microsoft.Extensions.Logging;
using Moq;
using Oip.Users.Base.Settings;
using Oip.Users.Base.StartupTasks;

namespace Oip.Test;

[TestFixture]
public class KeycloakSyncStartupTaskTests
{
    [Test]
    public void SyncOnStartup_IsEnabledByDefault()
    {
        Assert.That(new KeycloakSyncSettings().SyncOnStartup, Is.True);
    }

    [Test]
    public async Task ExecuteAsync_SyncOnStartupDisabled_SkipsSynchronizationAndLogsIt()
    {
        var loggerMock = new Mock<ILogger<KeycloakSyncStartupTask>>();
        // The synchronization service is not touched when the synchronization is skipped.
        var task = new KeycloakSyncStartupTask(null!, new KeycloakSyncSettings { SyncOnStartup = false },
            loggerMock.Object);

        await task.ExecuteAsync();

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("startup task skipped")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
