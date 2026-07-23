using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.DataModel.Response;
using Moq;
using Oip.Discussions.Base.Services;
using Oip.Discussions.Base.Settings;

namespace Oip.Test;

[TestFixture]
public class MinioDiscussionAttachmentStorageTests
{
    private const string BucketName = "discussion-bucket";

    private Mock<IMinioClient> _minioClientMock;
    private Mock<ILogger<MinioDiscussionAttachmentStorage>> _loggerMock;
    private MinioDiscussionAttachmentStorage _storage;

    [SetUp]
    public void SetUp()
    {
        _minioClientMock = new Mock<IMinioClient>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<MinioDiscussionAttachmentStorage>>();

        var settings = new DiscussionAttachmentStorageSettings
        {
            Endpoint = "localhost:9010",
            AccessKey = "admin",
            SecretKey = "password",
            BucketName = BucketName
        };

        _storage = new MinioDiscussionAttachmentStorage(
            new DiscussionAttachmentMinioClient(_minioClientMock.Object),
            settings,
            _loggerMock.Object);
    }

    [Test]
    public async Task SaveAsync_UploadsObjectToConfiguredBucket()
    {
        const long objectTypeId = 11;
        const long objectId = 42;
        var file = CreateFormFile("report.pdf", "application/pdf", [1, 2, 3, 4]);
        PutObjectArgs capturedArgs = null!;

        _minioClientMock
            .Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _minioClientMock
            .Setup(x => x.PutObjectAsync(It.IsAny<PutObjectArgs>(), It.IsAny<CancellationToken>()))
            .Callback<PutObjectArgs, CancellationToken>((args, _) => capturedArgs = args)
            .ReturnsAsync((PutObjectResponse)null!);

        var result = await _storage.SaveAsync(objectTypeId, objectId, file);

        Assert.Multiple(() =>
        {
            Assert.That(result.FileName, Is.EqualTo("report.pdf"));
            Assert.That(result.ContentType, Is.EqualTo("application/pdf"));
            Assert.That(result.Length, Is.EqualTo(4));
            Assert.That(result.StoragePath, Is.EqualTo($"discussions/{objectTypeId}/{objectId}/attachments/{result.StorageFileId}.pdf"));
            Assert.That(GetStringProperty(capturedArgs, "BucketName"), Is.EqualTo(BucketName));
            Assert.That(GetStringProperty(capturedArgs, "ObjectName"), Is.EqualTo(result.StoragePath));
            Assert.That(GetStringProperty(capturedArgs, "ContentType"), Is.EqualTo("application/pdf"));
            Assert.That(GetLongProperty(capturedArgs, "ObjectSize"), Is.EqualTo(4));
        });
    }

    [Test]
    public async Task OpenReadAsync_ReturnsDownloadedContentAndMetadata()
    {
        const string objectName = "discussions/attachments/2026/07/13/test.txt";
        const string contentType = "text/plain";
        const string fileName = "test.txt";
        var payload = "hello from minio"u8.ToArray();
        GetObjectArgs capturedArgs = null!;

        _minioClientMock
            .Setup(x => x.GetObjectAsync(It.IsAny<GetObjectArgs>(), It.IsAny<CancellationToken>()))
            .Callback<GetObjectArgs, CancellationToken>((args, _) =>
            {
                capturedArgs = args;
                InvokeStreamCallback(args, new MemoryStream(payload)).GetAwaiter().GetResult();
            })
            .ReturnsAsync((ObjectStat)null!);

        var result = await _storage.OpenReadAsync(objectName, contentType, fileName);
        await using var stream = result.Content;
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.ContentType, Is.EqualTo(contentType));
            Assert.That(result.FileName, Is.EqualTo(fileName));
            Assert.That(content, Is.EqualTo("hello from minio"));
            Assert.That(GetStringProperty(capturedArgs, "BucketName"), Is.EqualTo(BucketName));
            Assert.That(GetStringProperty(capturedArgs, "ObjectName"), Is.EqualTo(objectName));
        });
    }

    [Test]
    public async Task DeleteAsync_RemovesObjectFromConfiguredBucket()
    {
        const string objectName = "discussions/attachments/2026/07/13/delete.bin";
        RemoveObjectArgs capturedArgs = null!;

        _minioClientMock
            .Setup(x => x.RemoveObjectAsync(It.IsAny<RemoveObjectArgs>(), It.IsAny<CancellationToken>()))
            .Callback<RemoveObjectArgs, CancellationToken>((args, _) => capturedArgs = args)
            .Returns(Task.CompletedTask);

        await _storage.DeleteAsync(objectName);

        Assert.Multiple(() =>
        {
            Assert.That(GetStringProperty(capturedArgs, "BucketName"), Is.EqualTo(BucketName));
            Assert.That(GetStringProperty(capturedArgs, "ObjectName"), Is.EqualTo(objectName));
        });
    }

    [Test]
    public void SaveAsync_WhenMinioFails_LogsAndRethrows()
    {
        var file = CreateFormFile("error.bin", "application/octet-stream", [1]);
        var exception = new InvalidOperationException("save failed");

        _minioClientMock
            .Setup(x => x.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _minioClientMock
            .Setup(x => x.PutObjectAsync(It.IsAny<PutObjectArgs>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var thrown = Assert.ThrowsAsync<InvalidOperationException>(() => _storage.SaveAsync(11, 42, file));

        Assert.That(thrown, Is.SameAs(exception));
        VerifyErrorLogged("Failed to save discussion attachment");
    }

    [Test]
    public void OpenReadAsync_WhenMinioFails_LogsAndRethrows()
    {
        var exception = new InvalidOperationException("read failed");

        _minioClientMock
            .Setup(x => x.GetObjectAsync(It.IsAny<GetObjectArgs>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var thrown = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _storage.OpenReadAsync("discussions/attachments/fail.txt", "text/plain", "fail.txt"));

        Assert.That(thrown, Is.SameAs(exception));
        VerifyErrorLogged("Failed to read discussion attachment");
    }

    [Test]
    public void DeleteAsync_WhenMinioFails_LogsAndRethrows()
    {
        var exception = new InvalidOperationException("delete failed");

        _minioClientMock
            .Setup(x => x.RemoveObjectAsync(It.IsAny<RemoveObjectArgs>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var thrown = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _storage.DeleteAsync("discussions/attachments/fail.txt"));

        Assert.That(thrown, Is.SameAs(exception));
        VerifyErrorLogged("Failed to delete discussion attachment");
    }

    private void VerifyErrorLogged(string expectedMessage)
    {
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((value, _) => value.ToString()!.Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private static FormFile CreateFormFile(string fileName, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static string GetStringProperty(object instance, string propertyName) =>
        GetPropertyValue(instance, propertyName) as string;

    private static long GetLongProperty(object instance, string propertyName) =>
        Convert.ToInt64(GetPropertyValue(instance, propertyName));

    private static object GetPropertyValue(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.That(property, Is.Not.Null, $"Property '{propertyName}' was not found on {instance.GetType().Name}.");
        return property!.GetValue(instance)!;
    }

    private static Task InvokeStreamCallback(GetObjectArgs args, Stream source)
    {
        var callback = GetPropertyValue(args, "CallBack") as Func<Stream, CancellationToken, Task>;
        Assert.That(callback, Is.Not.Null, "GetObjectArgs callback was not configured.");
        return callback!(source, CancellationToken.None);
    }
}
