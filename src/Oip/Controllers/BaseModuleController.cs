using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Oip.Controllers.Api;
using Oip.Data.Dtos;
using Oip.Data.Repositories;

namespace Oip.Controllers;

/// <summary>
/// Base controller for module
/// </summary>
public abstract class BaseModuleController<TSettings> : Controller, IModuleControllerSecurity where TSettings : class
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
    [HttpGet]
    [Authorize(Roles = "admin")]
    [Route("get-security")]
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
    [HttpPut]
    [Authorize(Roles = "admin")]
    [Route("put-security")]
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
    [HttpGet("get-module-instance-settings")]
    [Authorize]
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