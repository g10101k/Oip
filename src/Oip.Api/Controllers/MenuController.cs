using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Exceptions;
using Oip.Base.Services;
using Oip.Data.Constants;
using Oip.Data.Dtos;
using Oip.Data.Repositories;

namespace Oip.Api.Controllers;

/// <summary>
/// API controller for retrieving and managing application menus.
/// </summary>
[ApiController]
[Route("api/menu")]
[ApiExplorerSettings(GroupName = "base")]
public class MenuController(ModuleRepository moduleRepository, UserService userService) : ControllerBase
{
    /// <summary>
    /// Retrieves the menu available to the current authenticated user.
    /// </summary>
    /// <returns>A list of <see cref="ModuleInstanceDto"/> objects available to the user.</returns>
    [Authorize, HttpGet("get")]
    public async Task<IEnumerable<ModuleInstanceDto>> Get()
    {
        return await moduleRepository.GetModuleForMenuAll(userService.GetUserRoles());
    }

    /// <summary>
    /// Retrieves the admin-specific menu.
    /// </summary>
    /// <returns>A list of <see cref="ModuleInstanceDto"/> objects representing the admin menu.</returns>
    [HttpGet("get-admin-menu")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task<IEnumerable<ModuleInstanceDto>> GetAdminMenu()
    {
        return await moduleRepository.GetAdminMenu();
    }

    /// <summary>
    /// Retrieves all available modules in the system.
    /// </summary>
    /// <returns>A list of <see cref="IntKeyValueDto"/> representing available modules.</returns>
    [HttpGet("get-modules")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public async Task<IEnumerable<IntKeyValueDto>> GetModules()
    {
        return await moduleRepository.GetModules();
    }

    /// <summary>
    /// Adds a new module instance to the system.
    /// </summary>
    /// <param name="addModuleInstanceDto">The data transfer object containing information about the new module instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [HttpPost("add-module-instance")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task AddModuleInstance(AddModuleInstanceDto addModuleInstanceDto)
    {
        await moduleRepository.AddModuleInstance(addModuleInstanceDto);
    }

    /// <summary>
    /// Edits an existing module instance.
    /// </summary>
    /// <param name="editModel">The data transfer object containing the updated module instance information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [HttpPost("edit-module-instance")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task EditModuleInstance(EditModuleInstanceDto editModel)
    {
        await moduleRepository.EditModuleInstance(editModel);
    }

    /// <summary>
    /// Deletes a module instance by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the module instance to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [HttpDelete("delete-module-instance")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task DeleteModuleInstance(int id)
    {
        await moduleRepository.DeleteModuleInstance(id);
    }

    /// <summary>
    /// Swaps the order positions of two modules in the menu structure.
    /// </summary>
    /// <param name="firstModuleId">The identifier of the first module to swap.</param>
    /// <param name="secondModuleId">The identifier of the second module to swap with.</param>
    /// <returns>A task that represents the asynchronous swap operation.</returns>
    [Authorize(Roles = SecurityConstants.AdminRole), HttpPost("change-order")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task ChangeModuleOrder(int firstModuleId, int secondModuleId)
    {
        await moduleRepository.ChangeModuleOrder(firstModuleId, secondModuleId);
    }
}