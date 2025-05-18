using Oip.Base.Helpers;

namespace Oip.Test;

[TestFixture]
public class UrlExtensionsTests
{
    /// <summary>
    /// Проверяет, что метод корректно объединяет URL и часть пути,
    /// когда оба не содержат лишних слешей в месте соединения.
    /// Ожидаемый результат: "https://example.com/api/users".
    /// </summary>
    [Test]
    public void UrlAppend_ShouldCorrectlyJoinUrlAndPart()
    {
        // Arrange
        string baseUrl = "https://example.com/api";
        string part = "users";

        // Act
        string result = baseUrl.UrlAppend(part);

        // Assert
        Assert.That(result, Is.EqualTo("https://example.com/api/users"));
    }

    /// <summary>
    /// Проверяет, что метод удаляет лишний слеш в конце базового URL.
    /// Ожидаемый результат: "https://example.com/api/users" (без дублирования слеша).
    /// </summary>
    [Test]
    public void UrlAppend_ShouldTrimSlashes_WhenBaseUrlEndsWithSlash()
    {
        // Arrange
        string baseUrl = "https://example.com/api/";
        string part = "users";

        // Act
        string result = baseUrl.UrlAppend(part);

        // Assert
        Assert.That(result, Is.EqualTo("https://example.com/api/users"));
    }

    /// <summary>
    /// Проверяет, что метод удаляет лишний слеш в начале добавляемой части пути.
    /// Ожидаемый результат: "https://example.com/api/users" (без дублирования слеша).
    /// </summary>
    [Test]
    public void UrlAppend_ShouldTrimSlashes_WhenPartStartsWithSlash()
    {
        // Arrange
        string baseUrl = "https://example.com/api";
        string part = "/users";

        // Act
        string result = baseUrl.UrlAppend(part);

        // Assert
        Assert.That(result, Is.EqualTo("https://example.com/api/users"));
    }

    /// <summary>
    /// Проверяет обработку случая, когда добавляемая часть пути пуста.
    /// Ожидаемый результат: "https://example.com/api/" (добавляется только слеш).
    /// </summary>
    [Test]
    public void UrlAppend_ShouldHandleEmptyPart()
    {
        // Arrange
        string baseUrl = "https://example.com/api";
        string part = "";

        // Act
        string result = baseUrl.UrlAppend(part);

        // Assert
        Assert.That(result, Is.EqualTo("https://example.com/api/"));
    }

    /// <summary>
    /// Проверяет обработку случая, когда и базовый URL, и добавляемая часть
    /// содержат слеши в месте соединения. Ожидается, что лишние слеши будут удалены.
    /// Ожидаемый результат: "https://example.com/api/users" (без дублирования слешей).
    /// </summary>
    [Test]
    public void UrlAppend_ShouldHandleBothSlashes()
    {
        // Arrange
        string baseUrl = "https://example.com/api/";
        string part = "/users";

        // Act
        string result = baseUrl.UrlAppend(part);

        // Assert
        Assert.That(result, Is.EqualTo("https://example.com/api/users"));
    }

    /// <summary>
    /// Проверяет, что метод выбрасывает ArgumentNullException, если добавляемая часть — null.
    /// </summary>
    [Test]
    public void UrlAppend_ShouldThrow_WhenPartIsNull()
    {
        // Arrange
        string baseUrl = "https://example.com/api";
        string part = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => baseUrl.UrlAppend(part));
    }
}