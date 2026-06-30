using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi.Models;
using Moq;
using Oip.Base.Settings;
using Oip.Base.StartupTasks;
using Swashbuckle.AspNetCore.Swagger;

namespace Oip.Test;

[TestFixture]
public class SwaggerGenerateWebClientStartupTaskTests
{
    private string _contentRootPath;
    private Mock<ISwaggerProvider> _swaggerProviderMock;
    private Mock<IWebHostEnvironment> _environmentMock;
    private Mock<IHostApplicationLifetime> _lifetimeMock;
    private Mock<IBaseOipModuleAppSettings> _settingsMock;
    private OpenApiItem _openApiItem;
    private OpenApiDocument _swaggerDocument;

    [SetUp]
    public void SetUp()
    {
        _contentRootPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_contentRootPath);

        _openApiItem = new OpenApiItem
        {
            Name = "v1",
            GenerateCommand = "generate {SwaggerJsonPath}"
        };

        _swaggerDocument = new OpenApiDocument
        {
            Info = new OpenApiInfo
            {
                Title = "Test API",
                Version = "v1"
            },
            Paths = new OpenApiPaths()
        };

        _swaggerProviderMock = new Mock<ISwaggerProvider>();
        _swaggerProviderMock.Setup(x => x.GetSwagger(_openApiItem.Name, null, null))
            .Returns(_swaggerDocument);

        _environmentMock = new Mock<IWebHostEnvironment>();
        _environmentMock.Setup(x => x.EnvironmentName).Returns(Environments.Development);
        _environmentMock.Setup(x => x.ContentRootPath).Returns(_contentRootPath);

        _lifetimeMock = new Mock<IHostApplicationLifetime>();

        _settingsMock = new Mock<IBaseOipModuleAppSettings>();
        _settingsMock.Setup(x => x.OpenApi).Returns(new OpenApiSettings { _openApiItem });
        _settingsMock.Setup(x => x.SpaProxyServer).Returns(new SpaDevelopmentServerSettings
        {
            WorkingDirectory = _contentRootPath
        });
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_contentRootPath))
            Directory.Delete(_contentRootPath, true);
    }

    [Test]
    public async Task ExecuteAsync_GenerationSucceeds_WritesFinalSwaggerSnapshot()
    {
        var task = CreateTask();

        await task.ExecuteAsync();

        var finalPath = GetFinalSwaggerPath();
        Assert.That(File.Exists(finalPath), Is.True);
        Assert.That(File.ReadAllText(finalPath), Is.EqualTo(task.GeneratedSwaggerJsonContents.Single()));
        Assert.That(task.GeneratedSwaggerJsonPaths.Single(), Does.Contain(Path.Combine("obj", "SwaggerFiles", "tmp")));
        Assert.That(File.Exists(task.GeneratedSwaggerJsonPaths.Single()), Is.False);
    }

    [Test]
    public void ExecuteAsync_GenerationFails_DoesNotUpdateFinalSwaggerSnapshotAndThrows()
    {
        var finalPath = GetFinalSwaggerPath();
        Directory.CreateDirectory(Path.GetDirectoryName(finalPath)!);
        File.WriteAllText(finalPath, "old swagger");

        var task = CreateTask(throwOnGenerate: true);

        Assert.ThrowsAsync<InvalidOperationException>(() => task.ExecuteAsync());
        Assert.That(File.ReadAllText(finalPath), Is.EqualTo("old swagger"));
        Assert.That(task.GeneratedSwaggerJsonPaths, Has.Count.EqualTo(1));
        Assert.That(File.Exists(task.GeneratedSwaggerJsonPaths.Single()), Is.False);
    }

    [Test]
    public async Task ExecuteAsync_SwaggerUnchangedAndForceGenerationDisabled_DoesNotGenerateClient()
    {
        var firstRunTask = CreateTask();
        await firstRunTask.ExecuteAsync();

        var secondRunTask = CreateTask();
        await secondRunTask.ExecuteAsync();

        Assert.That(secondRunTask.GenerateCallCount, Is.EqualTo(0));
    }

    [Test]
    public async Task ExecuteAsync_SwaggerUnchangedAndForceGenerationEnabled_GeneratesClient()
    {
        var firstRunTask = CreateTask();
        await firstRunTask.ExecuteAsync();

        _openApiItem.ForceGeneration = true;
        var secondRunTask = CreateTask();
        await secondRunTask.ExecuteAsync();

        Assert.That(secondRunTask.GenerateCallCount, Is.EqualTo(1));
    }

    private TestSwaggerGenerateWebClientStartupTask CreateTask(bool throwOnGenerate = false)
    {
        return new TestSwaggerGenerateWebClientStartupTask(
            _swaggerProviderMock.Object,
            _environmentMock.Object,
            _settingsMock.Object,
            _lifetimeMock.Object,
            throwOnGenerate);
    }

    private string GetFinalSwaggerPath()
    {
        return Path.Combine(_contentRootPath, "obj", "SwaggerFiles", "swagger-v1.json");
    }

    private sealed class TestSwaggerGenerateWebClientStartupTask(
        ISwaggerProvider swaggerProvider,
        IWebHostEnvironment environment,
        IBaseOipModuleAppSettings settings,
        IHostApplicationLifetime lifetime,
        bool throwOnGenerate) : SwaggerGenerateWebClientStartupTask(
        swaggerProvider,
        environment,
        NullLogger<SwaggerGenerateWebClientStartupTask>.Instance,
        lifetime,
        settings)
    {
        public int GenerateCallCount { get; private set; }

        public List<string> GeneratedSwaggerJsonPaths { get; } = new();

        public List<string> GeneratedSwaggerJsonContents { get; } = new();

        protected override Task GenerateTypeScriptClient(
            OpenApiItem config,
            string swaggerJsonPath,
            CancellationToken cancellationToken)
        {
            GenerateCallCount++;
            GeneratedSwaggerJsonPaths.Add(swaggerJsonPath);
            GeneratedSwaggerJsonContents.Add(File.ReadAllText(swaggerJsonPath));

            if (throwOnGenerate)
                throw new InvalidOperationException("Generation failed.");

            return Task.CompletedTask;
        }
    }
}
