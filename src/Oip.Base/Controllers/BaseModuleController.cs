using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
    : ControllerBase where TSettings : class
{
    /// <summary>
    /// Gets the security configuration for the specified module instance ID.
    /// </summary>
    /// <param name="id">The ID of the module instance.</param>
    /// <returns>A list of <see cref="SecurityResponse"/> objects representing the security rights and associated roles.</returns>
    [Authorize, HttpGet("get-security")]
    [Right(SecurityConstants.Read)]
    [ProducesResponseType<List<SecurityResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<List<SecurityResponse>> GetSecurity(int id)
    {
        var roleRightPair = await moduleRepository.GetSecurityByInstanceId(id);
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
    /// Gets the settings for the specified module instance.
    /// </summary>
    /// <param name="id">The ID of the module instance.</param>
    /// <returns>An <see cref="IActionResult"/> containing the deserialized settings object.</returns>
    [Authorize, HttpGet("get-module-instance-settings")]
    [Right(SecurityConstants.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public ActionResult<TSettings> GetModuleInstanceSettings(int id)
    {
        var settingString = moduleRepository.GetModuleInstanceSettings(id);
        var result = JsonConvert.DeserializeObject<TSettings>(settingString) ??
                     Activator.CreateInstance(typeof(TSettings)) as TSettings;
        return Ok(result);
    }

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
        var settingString = JsonConvert.SerializeObject(request.Settings);
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
    /// <param name="source">Where to read the module instance identifier from.</param>
    /// <returns><c>true</c> when the right is granted; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// Use this for partial restrictions inside an endpoint, such as masking a field. To deny the whole endpoint,
    /// use <see cref="RightAttribute" /> instead.
    /// </remarks>
    protected async Task<bool> HasInstanceRight(string right,
        ModuleInstanceIdSource source = ModuleInstanceIdSource.Header)
    {
        var moduleInstanceId = ModuleInstanceIdResolver.Resolve(Request, RouteData, source);
        if (moduleInstanceId is null)
            return false;

        var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        return await moduleRepository.HasInstanceRight(moduleInstanceId.Value, roles, right);
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
