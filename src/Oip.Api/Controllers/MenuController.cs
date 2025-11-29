using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Dtos;
using Oip.Base.Data.Repositories;
using Oip.Base.Services;

namespace Oip.Api.Controllers;

/// <summary>
/// API controller for retrieving and managing application menus.
/// </summary>
[ApiController]
[Route("api/menu")]
[ApiExplorerSettings(GroupName = "base")]
public class MenuController : ControllerBase
{
    private readonly ModuleRepository _moduleRepository;
    private readonly UserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuController"/> class.
    /// </summary>
    /// <param name="moduleRepository">The module repository instance for accessing module data.</param>
    /// <param name="userService">The user service instance for retrieving user roles and identity.</param>
    public MenuController(ModuleRepository moduleRepository, UserService userService)
    {
        _moduleRepository = moduleRepository;
        _userService = userService;
    }

    /// <summary>
    /// Retrieves the menu available to the current authenticated user.
    /// </summary>
    /// <returns>A list of <see cref="ModuleInstanceDto"/> objects available to the user.</returns>
    [HttpGet("get")]
    [Authorize]
    public async Task<IEnumerable<ModuleInstanceDto>> Get()
    {
        return await _moduleRepository.GetModuleForMenuAll(_userService.GetUserRoles());
    }

    /// <summary>
    /// Retrieves the admin-specific menu.
    /// </summary>
    /// <returns>A list of <see cref="ModuleInstanceDto"/> objects representing the admin menu.</returns>
    [HttpGet("get-admin-menu")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task<IEnumerable<ModuleInstanceDto>> GetAdminMenu()
    {
        return await _moduleRepository.GetAdminMenu();
    }

    /// <summary>
    /// Retrieves all available modules in the system.
    /// </summary>
    /// <returns>A list of <see cref="IntKeyValueDto"/> representing available modules.</returns>
    [HttpGet("get-modules")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task<IEnumerable<IntKeyValueDto>> GetModules()
    {
        return await _moduleRepository.GetModules();
    }

    /// <summary>
    /// Adds a new module instance to the system.
    /// </summary>
    /// <param name="addModuleInstanceDto">The data transfer object containing information about the new module instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [HttpPost("add-module-instance")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task AddModuleInstance(AddModuleInstanceDto addModuleInstanceDto)
    {
        await _moduleRepository.AddModuleInstance(addModuleInstanceDto);
    }

    /// <summary>
    /// Edits an existing module instance.
    /// </summary>
    /// <param name="editModel">The data transfer object containing the updated module instance information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [HttpPost("edit-module-instance")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task EditModuleInstance(EditModuleInstanceDto editModel)
    {
        await _moduleRepository.EditModuleInstance(editModel);
    }

    /// <summary>
    /// Deletes a module instance by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the module instance to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [HttpDelete("delete-module-instance")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task DeleteModuleInstance(int id)
    {
        await _moduleRepository.DeleteModuleInstance(id);
    }
}