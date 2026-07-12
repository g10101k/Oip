using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Oip.Base.Exceptions;
using Oip.Users.Base.Services;
using Oip.Users.Base.Settings;

namespace Oip.Users.Base.Controllers;

/// <summary>
/// Receives Keycloak events from the p2-inc/keycloak-events HTTP sender.
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/keycloak-events")]
public class KeycloakEventsController(
    KeycloakSyncService keycloakSyncService,
    KeycloakSyncSettings settings,
    ILogger<KeycloakEventsController> logger) : ControllerBase
{
    private const string SignatureHeaderName = "X-Keycloak-Signature";

    /// <summary>
    /// Receives a Keycloak user or admin event and synchronizes the local user profile.
    /// </summary>
    /// <returns>Accepted when the event was accepted or ignored.</returns>
    [HttpPost("receive-keycloak-event")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> ReceiveKeycloakEvent()
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync();
        if (string.IsNullOrWhiteSpace(body))
        {
            return BadRequest(new ApiExceptionResponse("Invalid event", "Request body is empty.",
                StatusCodes.Status400BadRequest));
        }

        if (!ValidateSignature(body))
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new ApiExceptionResponse("Forbidden", "Invalid Keycloak event signature.",
                    StatusCodes.Status403Forbidden));
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(body);
        }
        catch (JsonException)
        {
            return BadRequest(new ApiExceptionResponse("Invalid event", "Request body is not valid JSON.",
                StatusCodes.Status400BadRequest));
        }

        using (document)
        {
            if (TryGetUserDeleteEventId(document.RootElement, out var deletedUserId))
            {
                await keycloakSyncService.DeactivateUserFromKeycloak(deletedUserId);
                return Ok();
            }

            if (!TryGetUserChangeEventId(document.RootElement, out var keycloakUserId))
            {
                logger.LogDebug("Ignored unrelated Keycloak event: {Payload}", body);
                return Ok();
            }

            await keycloakSyncService.SyncUserFromKeycloak(keycloakUserId);
            return Ok();
        }
    }

    private bool ValidateSignature(string body)
    {
        if (string.IsNullOrWhiteSpace(settings.SharedSecret))
        {
            logger.LogWarning("Keycloak event shared secret is not configured.");
            return false;
        }

        var signature = Request.Headers[SignatureHeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        var expectedSignature = CalculateHmacSha256(body, settings.SharedSecret);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedSignature),
            Encoding.UTF8.GetBytes(signature.Trim()));
    }

    private static string CalculateHmacSha256(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool TryGetUserChangeEventId(JsonElement root, out string keycloakUserId)
    {
        keycloakUserId = string.Empty;

        if (TryGetAdminUserEvent(root, out keycloakUserId, out var operationType))
        {
            return IsAdminUserChangeOperation(operationType);
        }

        if (root.TryGetProperty("userId", out var userId) &&
            userId.ValueKind == JsonValueKind.String &&
            root.TryGetProperty("type", out var type) &&
            type.ValueKind == JsonValueKind.String &&
            IsUserChangeEventType(type.GetString()))
        {
            keycloakUserId = userId.GetString() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(keycloakUserId);
        }

        return false;
    }

    private static bool TryGetUserDeleteEventId(JsonElement root, out string keycloakUserId)
    {
        keycloakUserId = string.Empty;

        if (TryGetAdminUserEvent(root, out keycloakUserId, out var operationType))
        {
            return string.Equals(operationType, "DELETE", StringComparison.OrdinalIgnoreCase);
        }

        if (root.TryGetProperty("userId", out var userId) &&
            userId.ValueKind == JsonValueKind.String &&
            root.TryGetProperty("type", out var type) &&
            type.ValueKind == JsonValueKind.String &&
            string.Equals(type.GetString(), "DELETE_ACCOUNT", StringComparison.OrdinalIgnoreCase))
        {
            keycloakUserId = userId.GetString() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(keycloakUserId);
        }

        return false;
    }

    private static bool TryGetAdminUserEvent(JsonElement root, out string keycloakUserId, out string operationType)
    {
        keycloakUserId = string.Empty;
        operationType = string.Empty;

        if (!root.TryGetProperty("resourceType", out var resourceType) ||
            !string.Equals(resourceType.GetString(), "USER", StringComparison.OrdinalIgnoreCase) ||
            !root.TryGetProperty("operationType", out var operationTypeElement) ||
            operationTypeElement.ValueKind != JsonValueKind.String ||
            !root.TryGetProperty("resourcePath", out var resourcePath) ||
            resourcePath.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        var parsedUserId = ParseExactUserResourcePath(resourcePath.GetString());
        if (string.IsNullOrWhiteSpace(parsedUserId))
        {
            return false;
        }

        keycloakUserId = parsedUserId;
        operationType = operationTypeElement.GetString() ?? string.Empty;
        return true;
    }

    private static bool IsAdminUserChangeOperation(string? operationType)
    {
        return string.Equals(operationType, "CREATE", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(operationType, "UPDATE", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsUserChangeEventType(string? eventType)
    {
        return string.Equals(eventType, "REGISTER", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(eventType, "UPDATE_PROFILE", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ParseExactUserResourcePath(string? resourcePath)
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            return null;
        }

        var segments = resourcePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length == 2 &&
               string.Equals(segments[0], "users", StringComparison.OrdinalIgnoreCase)
            ? segments[1]
            : null;
    }
}