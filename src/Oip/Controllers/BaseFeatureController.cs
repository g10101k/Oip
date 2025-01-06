using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Oip.Controllers.Api;
using Oip.Data.Dtos;
using Oip.Data.Repositories;

namespace Oip.Controllers;

/// <summary>
/// Base controller for feature
/// </summary>
public abstract class BaseFeatureController<TSettings> : Controller, IFeatureControllerSecurity where TSettings : class
{
    /// <summary>
    /// Feature repository
    /// </summary>
    private readonly FeatureRepository _featureRepository;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="featureRepository"></param>
    protected BaseFeatureController(FeatureRepository featureRepository)
    {
        _featureRepository = featureRepository;
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
        var roleRightPair = await _featureRepository.GetSecurityByInstanceId(id);
        var result = new List<SecurityResponse>();
        foreach (var security in GetFeatureRights())
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
        List<FeatureSecurityDto> featureSecurityDtos = new();
        foreach (var security in request.Securities)
        {
            if (security.Roles is null) continue;
            foreach (var role in security.Roles)
            {
                featureSecurityDtos.Add(new FeatureSecurityDto()
                {
                    Right = security.Code,
                    Role = role
                });
            }
        }

        await _featureRepository.UpdateInstanceSecurity(request.Id, featureSecurityDtos);
        return Ok();
    }

    /// <summary>
    /// Get instance setting
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("get-feature-instance-settings")]
    [Authorize]
    public IActionResult GetFeatureInstanceSettings(int id)
    {
        var settingString = _featureRepository.GetFeatureInstanceSettings(id);
        var result = JsonConvert.DeserializeObject<TSettings>(settingString);
        if (result is null)
            result = Activator.CreateInstance(typeof(TSettings)) as TSettings;
        return Ok(result);
    }

    /// <summary>
    /// Save settings
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Roles = "admin")]
    [HttpPut("put-feature-instance-settings")]
    public void SaveSettings(SaveSettingsRequest request)
    {
        var settingString = JsonConvert.SerializeObject(request.Settings);
        _featureRepository.UpdateFeatureInstanceSettings(request.Id, settingString);
    }

    /// <inheritdoc />
    public abstract List<SecurityResponse> GetFeatureRights();

    /// <summary>
    /// Save settings request
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Settings"></param>
    public class SaveSettingsRequest
    {
        /// <summary>Feature instance id</summary>
        public int Id { get; set; }

        /// <summary>Settings</summary>
        public TSettings Settings { get; set; }
    }
}