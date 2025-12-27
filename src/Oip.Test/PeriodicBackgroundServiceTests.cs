using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Oip.Base.Services;

namespace Oip.Test;

[TestFixture]
public class PeriodicBackgroundServiceTests
{
    private Mock<IServiceScopeFactory> _scopeFactoryMock;
    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<ILogger<TestWorker>> _loggerMock;
    private CancellationTokenSource _cancellationTokenSource;

    [SetUp]
    public void SetUp()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<TestWorker>>();
        _cancellationTokenSource = new CancellationTokenSource();

        _scopeFactoryMock.Setup(x => x.CreateScope())
            .Returns(scopeMock.Object);
        scopeMock.Setup(x => x.ServiceProvider)
            .Returns(_serviceProviderMock.Object);
    }

    [Test]
    public async Task ExecuteAsync_WorkerCreatedSuccessfully_ExecutesWorker()
    {
        // Arrange
        var worker = new TestWorker(30);
        _serviceProviderMock.Setup(x => x.GetService(typeof(TestWorker)))
            .Returns(worker);

        var service = new PeriodicBackgroundService<TestWorker>(
            _scopeFactoryMock.Object, _loggerMock.Object);

        // Act
        var task = service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(100);
        await service.StopAsync(_cancellationTokenSource.Token);

        // Assert
        Assert.That(worker.Executed, Is.True);
        VerifyLogInformation("Service started.");
        VerifyLogDebug("Work started at:");
    }

    [Test]
    public async Task ExecuteAsync_WorkerIsNull_Waits60Seconds()
    {
        // Arrange
        _serviceProviderMock.Setup(x => x.GetService(typeof(TestWorker)))
            .Returns((TestWorker)null);

        var service = new PeriodicBackgroundService<TestWorker>(_scopeFactoryMock.Object, _loggerMock.Object);

        // Act
        _ = service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(100);
        await service.StopAsync(_cancellationTokenSource.Token);

        // Assert
        VerifyLogWarning("Worker is null. Waiting 60 seconds.");
    }

    [Test]
    public async Task ExecuteAsync_IntervalNegative_Waits60Seconds()
    {
        // Arrange
        var worker = new TestWorker(-1);
        _serviceProviderMock.Setup(x => x.GetService(typeof(TestWorker)))
            .Returns(worker);

        var service = new PeriodicBackgroundService<TestWorker>(
            _scopeFactoryMock.Object, _loggerMock.Object);

        // Act
        var task = service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(100);
        await service.StopAsync(_cancellationTokenSource.Token);

        // Assert
        VerifyLogInformation("Interval <= 0, worker not accepted. Waiting 60 seconds.");
        Assert.That(worker.Executed, Is.False);
    }

    [Test]
    public async Task ExecuteAsync_WorkerThrowsOperationCanceled_LogsErrorAndContinues()
    {
        // Arrange
        var worker = new TestWorker(10) { ThrowOperationCanceled = true };
        _serviceProviderMock.Setup(x => x.GetService(typeof(TestWorker)))
            .Returns(worker);

        var service = new PeriodicBackgroundService<TestWorker>(
            _scopeFactoryMock.Object, _loggerMock.Object);

        // Act
        var task = service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(100);
        await service.StopAsync(_cancellationTokenSource.Token);

        // Assert
        VerifyLogInformation("Service worker canceled.");
    }

    [Test]
    public async Task ExecuteAsync_WorkerThrowsException_LogsErrorAndContinues()
    {
        // Arrange
        var worker = new TestWorker(10) { ThrowException = true };
        _serviceProviderMock.Setup(x => x.GetService(typeof(TestWorker)))
            .Returns(worker);

        var service = new PeriodicBackgroundService<TestWorker>(
            _scopeFactoryMock.Object, _loggerMock.Object);

        // Act
        var task = service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(100);
        await service.StopAsync(_cancellationTokenSource.Token);

        // Assert
        VerifyLogError("PeriodicService failed.");
    }

    [Test]
    public async Task ExecuteAsync_CancellationRequested_StopsService()
    {
        // Arrange
        var worker = new TestWorker(60); // Long interval
        _serviceProviderMock.Setup(x => x.GetService(typeof(TestWorker)))
            .Returns(worker);

        var service = new PeriodicBackgroundService<TestWorker>(
            _scopeFactoryMock.Object, _loggerMock.Object);

        // Act
        var task = service.StartAsync(_cancellationTokenSource.Token);
        await Task.Delay(100);
        await _cancellationTokenSource.CancelAsync();
        await Task.Delay(100);

        // Assert
        VerifyLogInformation("Service worker canceled.");
    }

    private void VerifyLogInformation(string expectedMessage)
    {
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    private void VerifyLogWarning(string expectedMessage)
    {
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    private void VerifyLogDebug(string expectedMessage)
    {
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    private void VerifyLogError(string expectedMessage)
    {
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }
}

// Test implementation for testing
public class TestWorker(int interval) : IPeriodicalService
{
    public int Interval { get; } = interval;
    public bool Executed { get; private set; }
    public bool ThrowOperationCanceled { get; set; }
    public bool ThrowException { get; set; }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (ThrowOperationCanceled)
            throw new OperationCanceledException();

        if (ThrowException)
            throw new InvalidOperationException("Test exception");

        Executed = true;
        await Task.Delay(10, cancellationToken);
    }
}