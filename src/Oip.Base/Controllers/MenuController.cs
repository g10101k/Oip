using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Data.Dtos;
using Oip.Base.Data.Repositories;
using Oip.Base.Services;

namespace Oip.Base.Controllers;

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
    
    /// <summary>
    /// Add new module
    /// </summary>  
    /// <returns></returns> 
    [HttpPost("edit-module-instance")]
    [Authorize(Roles = "admin")]
    public async Task EditModuleInstance(EditModuleInstanceDto editModel)
    {
        await _moduleRepository.EditModuleInstance(editModel);
    }
    
    /// <summary>
    /// Add new module
    /// </summary>  
    /// <returns></returns> 
    [HttpDelete("delete-module-instance")]
    [Authorize(Roles = "admin")]
    public async Task DeleteModuleInstance(int id)
    {
        await _moduleRepository.DeleteModuleInstance(id);
    }
}

