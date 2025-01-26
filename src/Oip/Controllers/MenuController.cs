using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Services;
using Oip.Data.Dtos;
using Oip.Data.Repositories;

namespace Oip.Controllers;

/// <summary>
/// Menu controller
/// </summary>
[ApiController]
[Route("api/menu")]
public class MenuController : ControllerBase
{
    private readonly ModuleRepository _moduleRepository;
    private readonly UserService _userService;

    /// <summary>
    /// .ctor
    /// </summary>
    public MenuController(ModuleRepository moduleRepository, UserService userService)
    {
        _moduleRepository = moduleRepository;
        _userService = userService;
    }

    /// <summary>
    /// Get menu for client app
    /// </summary>
    /// <returns></returns>
    [HttpGet("get")]
    [Authorize]
    public async Task<IEnumerable<ModuleInstanceDto>> Get()
    {
        return await _moduleRepository.GetModuleForMenuAll(_userService.GetUserRoles());
    }

    /// <summary>
    /// Get admin menu for client app
    /// </summary>  
    /// <returns></returns>
    [HttpGet("get-admin-menu")]
    [Authorize(Roles = "admin")]
    public async Task<IEnumerable<ModuleInstanceDto>> GetAdminMenu()
    {
        return await _moduleRepository.GetAdminMenu();
    }
    
    /// <summary>
    /// Get admin menu for client app
    /// </summary>  
    /// <returns></returns>
    [HttpGet("get-modules")]
    [Authorize(Roles = "admin")]
    public async Task<IEnumerable<IntKeyValueDto>> GetModules()
    {
        return await _moduleRepository.GetModules();
    }
    
    /// <summary>
    /// Add new module
    /// </summary>  
    /// <returns></returns> 
    [HttpPost("add-module-instance")]
    [Authorize(Roles = "admin")]
    public async Task AddModuleInstance(AddModuleInstanceDto addModuleInstanceDto)
    {
         await _moduleRepository.AddModuleInstance(addModuleInstanceDto);
    }
}

