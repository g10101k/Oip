using System.Reflection;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Moq;
using Oip.Users.Base.Services;
using Oip.Users.Base.Settings;

namespace Oip.Test;

[TestFixture]
public class MinioUserPhotoStorageTests
{
    private const string BucketName = "user-photo-bucket";

    private Mock<IMinioClient> _minioClientMock;
    private Mock<ILogger<MinioUserPhotoStorage>> _loggerMock;
    private MinioUserPhotoStorage _storage;

    [SetUp]
    public void SetUp()
    {
        _minioClientMock = new Mock<IMinioClient>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<MinioUserPhotoStorage>>();

        var settings = new UserPhotoStorageSettings
        {
            Endpoint = "localhost:9010",
            AccessKey = "admin",
            SecretKey = "password",
            BucketName = BucketName
        };

        _storage = new MinioUserPhotoStorage(new UserPhotoMinioClient(_minioClientMock.Object), settings, _loggerMock.Object);
    }

    [Test]
    public void OpenReadAsync_WhenMinioFails_LogsAndRethrows()
    {
        var exception = new InvalidOperationException("read failed");

        _minioClientMock
            .Setup(x => x.GetObjectAsync(It.IsAny<GetObjectArgs>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var thrown = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _storage.OpenReadAsync("users/photos/fail", "image/jpeg"));

        Assert.That(thrown, Is.SameAs(exception));
        VerifyErrorLogged("Failed to read user photo");
    }

    [Test]
    public void OpenReadAsync_WhenMinioIsUnreachable_ThrowsConnectionExceptionInsteadOfEmptyStream()
    {
        // Regression guard: when MinIO is unreachable, the SDK must surface a ConnectionException
        // (not silently return an empty stream that our code would mistake for an empty object).
        var connectionException = new ConnectionException("Connection error: Connection refused");

        _minioClientMock
            .Setup(x => x.GetObjectAsync(It.IsAny<GetObjectArgs>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(connectionException);

        var thrown = Assert.ThrowsAsync<ConnectionException>(() =>
            _storage.OpenReadAsync("users/photos/unreachable", "image/jpeg"));

        Assert.That(thrown, Is.SameAs(connectionException));
        Assert.That(thrown!.Message, Does.Contain("Connection error"));
        VerifyErrorLogged("Failed to read user photo");
    }

    [Test]
    public async Task DeleteAsync_RemovesObjectFromConfiguredBucket()
    {
        const string objectName = "users/photos/42";
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
    public void DeleteAsync_WhenMinioFails_LogsAndRethrows()
    {
        var exception = new InvalidOperationException("delete failed");

        _minioClientMock
            .Setup(x => x.RemoveObjectAsync(It.IsAny<RemoveObjectArgs>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var thrown = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _storage.DeleteAsync("users/photos/fail"));

        Assert.That(thrown, Is.SameAs(exception));
        VerifyErrorLogged("Failed to delete user photo");
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

    private static string GetStringProperty(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.That(property, Is.Not.Null, $"Property '{propertyName}' was not found on {instance.GetType().Name}.");
        return (string)property!.GetValue(instance)!;
    }
}
