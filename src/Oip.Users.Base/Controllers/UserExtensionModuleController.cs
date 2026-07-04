using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oip.Api.Controllers;
using Oip.Base.Exceptions;
using Oip.Data.Constants;
using Oip.Data.Dtos;
using Oip.Data.Repositories;
using Oip.Users.Base.Dtos;
using Oip.Users.Base.Services;

namespace Oip.Users.Base.Controllers;

/// <summary>
/// Example module controller for user extension fields and extension-aware user table data.
/// </summary>
[ApiController]
[Authorize(Roles = SecurityConstants.AdminRole)]
[Route("api/user-extension-module")]
public class UserExtensionModuleController(
    UserExtensionTableService tableService,
    UserExtensionMetadataService metadataService,
    ModuleRepository moduleRepository)
    : BaseModuleController<UserExtensionModuleSettings>(moduleRepository)
{
    /// <summary>
    /// Gets a table page with base user fields and dynamic extension values.
    /// </summary>
    [HttpPost("get-user-extension-page")]
    [ProducesResponseType(typeof(ExtensionTablePageResult<UserExtensionTableRowDto>), StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExtensionTablePageResult<UserExtensionTableRowDto>>> GetUserExtensionPage(
        [FromBody] TableQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        return Ok(await tableService.GetPageAsync(request, cancellationToken));
    }

    /// <summary>
    /// Gets global user extension field metadata.
    /// </summary>
    [HttpGet("get-user-extension-fields")]
    [ProducesResponseType(typeof(List<ExtensionFieldMetadataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ExtensionFieldMetadataDto>>> GetUserExtensionFields(
        CancellationToken cancellationToken = default)
    {
        return Ok(await metadataService.GetFieldsAsync(cancellationToken));
    }

    /// <summary>
    /// Creates global user extension field metadata and a physical column.
    /// </summary>
    [HttpPost("create-user-extension-field")]
    [ProducesResponseType(typeof(ExtensionFieldMetadataDto), StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExtensionFieldMetadataDto>> CreateUserExtensionField(
        [FromBody] CreateUserExtensionFieldRequest request,
        CancellationToken cancellationToken = default)
    {
        return Ok(await metadataService.CreateFieldAsync(request, cancellationToken));
    }

    /// <summary>
    /// Updates global user extension field presentation metadata.
    /// </summary>
    [HttpPut("update-user-extension-field/{id:int}")]
    [ProducesResponseType(typeof(ExtensionFieldMetadataDto), StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExtensionFieldMetadataDto>> UpdateUserExtensionField(
        int id,
        [FromBody] UpdateUserExtensionFieldRequest request,
        CancellationToken cancellationToken = default)
    {
        return Ok(await metadataService.UpdateFieldAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Deletes global user extension field metadata and the physical column.
    /// </summary>
    [HttpDelete("delete-user-extension-field/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUserExtensionField(
        int id,
        CancellationToken cancellationToken = default)
    {
        await metadataService.DeleteFieldAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Updates extension values for a user.
    /// </summary>
    [HttpPut("update-user-extension-values/{userId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUserExtensionValues(
        int userId,
        [FromBody] UpdateUserExtensionValuesRequest request,
        CancellationToken cancellationToken = default)
    {
        await tableService.UpdateValuesAsync(userId, request.Values, cancellationToken);
        return NoContent();
    }
}

/// <summary>
/// Module settings for the user extension module.
/// </summary>
public class UserExtensionModuleSettings
{
}
