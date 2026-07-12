using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Dtos;
using Oip.Base.Data.Repositories;
using Oip.Base.Exceptions;
using Oip.Base.Properties;

namespace Oip.Base.Controllers;

/// <summary>
/// Runtime endpoints for Web Component extension instances and API proxying.
/// </summary>
[ApiController]
[Route("api/extensions")]
[ApiExplorerSettings(GroupName = "base")]
public class ExtensionsController(ModuleRepository moduleRepository, IHttpClientFactory httpClientFactory)
    : ControllerBase
{
    /// <summary>
    /// Gets the security configuration for an extension module instance.
    /// </summary>
    [Authorize, HttpGet("get-security")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<List<SecurityResponse>> GetSecurity(int id)
    {
        var roleRightPair = await moduleRepository.GetSecurityByInstanceId(id);
        var read = new SecurityResponse
        {
            Code = SecurityConstants.Read,
            Name = Resources.BaseModuleController_GetModuleRights_Read,
            Description = Resources.BaseModuleController_GetModuleRights_Can_view_this_module,
            Roles = roleRightPair
                .Where(x => x.Right == SecurityConstants.Read)
                .Select(x => x.Role)
                .Distinct()
                .ToList()
        };

        return [read];
    }

    /// <summary>
    /// Updates the security configuration for an extension module instance.
    /// </summary>
    [HttpPut("put-security")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> PutSecurity(PutSecurityRequest request)
    {
        List<ModuleSecurityDto> securityDtos = [];
        foreach (var security in request.Securities)
        {
            if (security.Roles is null) continue;
            securityDtos.AddRange(security.Roles.Select(role => new ModuleSecurityDto
            {
                Right = security.Code,
                Role = role
            }));
        }

        await moduleRepository.UpdateInstanceSecurity(request.Id, securityDtos);
        return Ok();
    }

    /// <summary>
    /// Gets raw settings for an extension module instance.
    /// </summary>
    [Authorize, HttpGet("get-module-instance-settings")]
    [ProducesResponseType<object>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public IActionResult GetModuleInstanceSettings(int id)
    {
        var settings = moduleRepository.GetModuleInstanceSettings(id);
        if (string.IsNullOrWhiteSpace(settings))
        {
            return Ok(new { });
        }

        return Content(settings, "application/json");
    }

    /// <summary>
    /// Saves settings for an extension module instance.
    /// </summary>
    [HttpPut("put-module-instance-settings")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public void SaveSettings(SaveExtensionSettingsRequest request)
    {
        var settingString = request.Settings.ValueKind == JsonValueKind.Undefined
            ? "{}"
            : request.Settings.GetRawText();
        moduleRepository.UpdateModuleInstanceSettings(request.Id, settingString);
    }

    /// <summary>
    /// Proxies extension backend API calls through OIP.
    /// </summary>
    [AcceptVerbs("GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS")]
    [Route("{extensionKey}/{**path}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task Proxy(string extensionKey, string? path)
    {
        var module = await moduleRepository.GetExtensionModuleByKey(extensionKey)
                     ?? throw new ApiException(
                         "Extension module not found",
                         $"Extension module with key '{extensionKey}' was not found.",
                         StatusCodes.Status404NotFound);

        if (string.IsNullOrWhiteSpace(module.ApiBaseUrl))
        {
            throw new ApiException("Extension API is not configured", null, StatusCodes.Status400BadRequest);
        }

        var targetUri = BuildTargetUri(module.ApiBaseUrl, path, Request.QueryString.Value);
        using var requestMessage = new HttpRequestMessage(new HttpMethod(Request.Method), targetUri);
        CopyRequestHeaders(requestMessage);

        if (Request.ContentLength > 0 || Request.Headers.ContainsKey("Transfer-Encoding"))
        {
            requestMessage.Content = new StreamContent(Request.Body);
            foreach (var header in Request.Headers)
            {
                if (header.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
                {
                    requestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }
        }

        using var responseMessage = await httpClientFactory.CreateClient().SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            HttpContext.RequestAborted);

        Response.StatusCode = (int)responseMessage.StatusCode;
        foreach (var header in responseMessage.Headers)
        {
            Response.Headers[header.Key] = header.Value.ToArray();
        }

        foreach (var header in responseMessage.Content.Headers)
        {
            Response.Headers[header.Key] = header.Value.ToArray();
        }

        Response.Headers.Remove("transfer-encoding");
        await responseMessage.Content.CopyToAsync(Response.Body, HttpContext.RequestAborted);
    }

    private void CopyRequestHeaders(HttpRequestMessage requestMessage)
    {
        foreach (var header in Request.Headers)
        {
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                header.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }

    private static Uri BuildTargetUri(string apiBaseUrl, string? path, string? queryString)
    {
        var normalizedBase = apiBaseUrl.EndsWith('/') ? apiBaseUrl : $"{apiBaseUrl}/";
        var normalizedPath = path?.TrimStart('/') ?? string.Empty;
        return new Uri($"{normalizedBase}{normalizedPath}{queryString}");
    }

    /// <summary>
    /// Request to save extension settings.
    /// </summary>
    public class SaveExtensionSettingsRequest
    {
        /// <summary>
        /// Module instance identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Settings payload.
        /// </summary>
        public JsonElement Settings { get; set; }
    }
}
