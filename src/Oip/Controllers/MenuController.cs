using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Services;
using Oip.Data.Repositories;

namespace Oip.Controllers;

/// <summary>
/// Menu controller
/// </summary>
[ApiController]
[Route("api/menu")]
public class MenuController : ControllerBase
{
    private readonly FeatureRepository _featureRepository;
    private readonly UserService _userService;

    /// <summary>
    /// .ctor
    /// </summary>
    public MenuController(FeatureRepository featureRepository, UserService userService)
    {
        _featureRepository = featureRepository;
        _userService = userService;
    }

    /// <summary>
    /// Get menu for client app
    /// </summary>
    /// <returns></returns>
    [HttpGet("get")]
    [Authorize]
    public async Task<IEnumerable<FeatureInstanceDto>> Get()
    {
        return await _featureRepository.GetFeatureForMenuAll(_userService.GetUserRoles());
    }
}