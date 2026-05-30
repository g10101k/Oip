using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oip.Applications.Base;
using Oip.Applications.Base.Contracts;
using Oip.Base.Exceptions;

namespace Oip.Applications.Controllers;

/// <summary>
/// API controller for frontend application registry.
/// </summary>
[ApiController]
[Route("api/applications")]
[ApiExplorerSettings(GroupName = "base")]
public class ApplicationsController(IApplicationRegistryService registryService) : ControllerBase
{
    /// <summary>
    /// Retrieves registered frontend applications.
    /// </summary>
    [Authorize]
    [HttpGet("get-application-registry-items")]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType<IReadOnlyList<ApplicationRegistryItemDto>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<ApplicationRegistryItemDto>> GetApplicationRegistryItems(
        CancellationToken cancellationToken = default)
    {
        return registryService.GetApplicationRegistryItemsAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a registered frontend application by code.
    /// </summary>
    [Authorize]
    [HttpGet("get-application-registry-item-by-code/{code}")]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public Task<ApplicationRegistryItemDto> GetApplicationRegistryItemByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        return registryService.GetApplicationRegistryItemByCodeAsync(code, cancellationToken);
    }

    /// <summary>
    /// Creates a registered frontend application.
    /// </summary>
    [Authorize]
    [HttpPost("create-application-registry-item")]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public Task<ApplicationRegistryItemDto> CreateApplicationRegistryItem(
        [FromBody] ApplicationRegistryItemDto application,
        CancellationToken cancellationToken = default)
    {
        return registryService.CreateApplicationRegistryItemAsync(application, cancellationToken);
    }

    /// <summary>
    /// Updates a registered frontend application by code.
    /// </summary>
    [Authorize]
    [HttpPut("update-application-registry-item/{code}")]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public Task<ApplicationRegistryItemDto> UpdateApplicationRegistryItem(
        string code,
        [FromBody] ApplicationRegistryItemDto application,
        CancellationToken cancellationToken = default)
    {
        return registryService.UpdateApplicationRegistryItemAsync(code, application, cancellationToken);
    }

    /// <summary>
    /// Deletes a registered frontend application by code.
    /// </summary>
    [Authorize]
    [HttpDelete("delete-application-registry-item/{code}")]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteApplicationRegistryItem(
        string code,
        CancellationToken cancellationToken = default)
    {
        await registryService.DeleteApplicationRegistryItemAsync(code, cancellationToken);
        return NoContent();
    }
}
