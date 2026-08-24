using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Dtos;
using Oip.Base.Data.Repositories;
using Oip.Base.Exceptions;
using Oip.Base.Properties;
using Oip.Base.Security;

namespace Oip.Base.Controllers;

/// <summary>
/// Base controller for module-specific operations.
/// Provides functionality to manage module settings and security.
/// </summary>
/// <typeparam name="TSettings">The type representing module settings.</typeparam>
public abstract class BaseModuleController<TSettings>(ModuleRepository moduleRepository)
    : ControllerBase where TSettings : class, new()
{
    /// <summary>
    /// Gets the security configuration for the module instance the request targets.
    /// </summary>
    /// <returns>A list of <see cref="SecurityResponse"/> objects representing the security rights and associated roles.</returns>
    [Authorize, HttpGet("get-security")]
    [Right(SecurityConstants.Read)]
    [ProducesResponseType<List<SecurityResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<List<SecurityResponse>> GetSecurity()
    {
        var roleRightPair = await moduleRepository.GetSecurityByInstanceId(GetModuleInstanceId());
        var result = new List<SecurityResponse>();
        foreach (var security in GetModuleRights())
        {
            security.Roles = roleRightPair.Where(x => x.Right == security.Code).Select(x => x.Role).Distinct().ToList();
            result.Add(security);
        }

        return result;
    }

    /// <summary>
    /// Updates the security configuration for the specified module instance.
    /// </summary>
    /// <param name="request">The request containing the new security configuration.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the operation.</returns>
    [HttpPut("put-security")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> PutSecurity(PutSecurityRequest request)
    {
        List<ModuleSecurityDto> securityDtos = new();
        foreach (var security in request.Securities)
        {
            if (security.Roles is null) continue;
            foreach (var role in security.Roles)
            {
                securityDtos.Add(new ModuleSecurityDto()
                {
                    Right = security.Code,
                    Role = role
                });
            }
        }

        await moduleRepository.UpdateInstanceSecurity(request.Id, securityDtos);
        return Ok();
    }

    /// <summary>
    /// Gets the settings for the module instance the request targets.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing the deserialized settings object.</returns>
    [Authorize, HttpGet("get-module-instance-settings")]
    [Right(SecurityConstants.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public ActionResult<TSettings> GetModuleInstanceSettings()
        => Ok(moduleRepository.GetModuleInstanceSettings<TSettings>(GetModuleInstanceId()));

    /// <summary>
    /// Saves the settings for the specified module instance.
    /// </summary>
    /// <param name="request">The request containing the new settings and instance ID.</param>
    [HttpPut("put-module-instance-settings")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public void SaveSettings(SaveSettingsRequest request)
    {
        var settingString = JsonSerializer.Serialize(request.Settings);
        moduleRepository.UpdateModuleInstanceSettings(request.Id, settingString);
    }

    /// <summary>
    /// Gets the list of security rights supported by the module.
    /// </summary>
    /// <returns>A list of <see cref="SecurityResponse"/> representing available rights.</returns>
    [HttpGet("get-module-rights")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType<List<SecurityResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public virtual List<SecurityResponse> GetModuleRights()
    {
        return new()
        {
            new()
            {
                Code = SecurityConstants.Read,
                Name = Resources.BaseModuleController_GetModuleRights_Read,
                Description = Resources.BaseModuleController_GetModuleRights_Can_view_this_module,
                Roles = [SecurityConstants.AdminRole]
            }
        };
    }

    /// <summary>
    /// Checks whether the current user holds the specified right on the module instance the request targets.
    /// </summary>
    /// <param name="right">The right to check, see <see cref="SecurityConstants" />.</param>
    /// <returns><c>true</c> when the right is granted; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// Use this for partial restrictions inside an endpoint, such as masking a field. To deny the whole endpoint,
    /// use <see cref="RightAttribute" /> instead.
    /// </remarks>
    protected async Task<bool> HasInstanceRight(string right)
    {
        var moduleInstanceId = Request.GetModuleInstanceId();
        if (moduleInstanceId is null)
            return false;

        var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        return await moduleRepository.HasInstanceRight(moduleInstanceId.Value, roles, right);
    }

    /// <summary>
    /// Gets the module instance identifier the request targets.
    /// </summary>
    /// <returns>The module instance identifier.</returns>
    /// <exception cref="ApiException">The request does not specify a module instance.</exception>
    protected int GetModuleInstanceId()
    {
        return Request.GetModuleInstanceId() ?? throw new ApiException("Module instance is not specified",
            "The request does not specify the module instance it targets.", StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Represents a request to save module instance settings.
    /// </summary>
    public class SaveSettingsRequest
    {
        /// <summary>
        /// Gets or sets the ID of the module instance.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the settings object to be saved.
        /// </summary>
        public TSettings Settings { get; set; } = null!;
    }
}
