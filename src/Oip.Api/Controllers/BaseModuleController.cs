using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Oip.Api.Controllers.Api;
using Oip.Api.Properties;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Dtos;
using Oip.Base.Data.Repositories;
using Oip.Base.Exceptions;

namespace Oip.Api.Controllers;

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
    [HttpGet("get-security")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType<List<SecurityResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<OipException>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<OipException>(StatusCodes.Status403Forbidden)]
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
    [ProducesResponseType<OipException>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<OipException>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<OipException>(StatusCodes.Status403Forbidden)]
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
    [HttpGet("get-module-instance-settings")]
    [Authorize]
    [ProducesResponseType<object>(StatusCodes.Status200OK)]
    [ProducesResponseType<OipException>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<OipException>(StatusCodes.Status403Forbidden)]
    public IActionResult GetModuleInstanceSettings(int id)
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
    [ProducesResponseType<OipException>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<OipException>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<OipException>(StatusCodes.Status403Forbidden)]
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
    [ProducesResponseType<OipException>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<OipException>(StatusCodes.Status403Forbidden)]
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
        public TSettings Settings { get; set; }
    }
}