using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Dtos;
using Oip.Base.Data.Repositories;

namespace Oip.Base.Controllers;

/// <summary>
/// Base controller for module
/// </summary>
public abstract class BaseModuleController<TSettings> : ControllerBase, IModuleControllerSecurity where TSettings : class
{
    /// <summary>
    /// Module repository
    /// </summary>
    private readonly ModuleRepository _moduleRepository;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="moduleRepository"></param>
    protected BaseModuleController(ModuleRepository moduleRepository)
    {
        _moduleRepository = moduleRepository;
    }

    /// <summary>
    /// Get security for instance id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("get-security")]
    [Authorize(Roles = "admin")]
    public async Task<List<SecurityResponse>> GetSecurity(int id)
    {
        var roleRightPair = await _moduleRepository.GetSecurityByInstanceId(id);
        var result = new List<SecurityResponse>();
        foreach (var security in GetModuleRights())
        {
            security.Roles = roleRightPair.Where(x => x.Right == security.Code).Select(x => x.Role).Distinct().ToList();
            result.Add(security);
        }

        return result;
    }

    /// <summary>
    /// Update security
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("put-security")]
    [Authorize(Roles = "admin")]
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

        await _moduleRepository.UpdateInstanceSecurity(request.Id, securityDtos);
        return Ok();
    }

    /// <summary>
    /// Get instance setting
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("get-module-instance-settings")]
    public IActionResult GetModuleInstanceSettings(int id)
    {
        var settingString = _moduleRepository.GetModuleInstanceSettings(id);
        var result = JsonConvert.DeserializeObject<TSettings>(settingString) ??
                     Activator.CreateInstance(typeof(TSettings)) as TSettings;
        return Ok(result);
    }

    /// <summary>
    /// Save settings
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Roles = "admin")]
    [HttpPut("put-module-instance-settings")]
    public void SaveSettings(SaveSettingsRequest request)
    {
        var settingString = JsonConvert.SerializeObject(request.Settings);
        _moduleRepository.UpdateModuleInstanceSettings(request.Id, settingString);
    }

    /// <inheritdoc />
    [Authorize(Roles = "admin")]
    [HttpGet("get-module-rights")]
    public abstract List<SecurityResponse> GetModuleRights();

    /// <summary>
    /// Save settings request
    /// </summary>
    public class SaveSettingsRequest
    {
        /// <summary>Module instance id</summary>
        public int Id { get; set; }

        /// <summary>Settings</summary>
        public TSettings Settings { get; set; }
    }
}