using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Repositories;
using Oip.Base.Exceptions;
using Oip.Base.Security;
using Oip.Hil.Controllers.Api;
using Oip.Hil.Services;

namespace Oip.Hil.Controllers;

/// <summary>
/// Module that manages LLM provider configurations (OpenAI-compatible APIs).
/// </summary>
[ApiController]
[Authorize]
[Route("api/llm-provider-module")]
public class LlmProviderModuleController(LlmProviderService providerService, ModuleRepository moduleRepository)
    : BaseModuleController<LlmProviderModuleSettings>(moduleRepository)
{
    /// <summary>
    /// Returns all configured providers. API keys are returned masked.
    /// </summary>
    [Right(SecurityConstants.Read), HttpGet("get-providers")]
    [ProducesResponseType<List<LlmProviderDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<LlmProviderDto>>> GetProviders(CancellationToken cancellationToken)
    {
        return Ok(await providerService.GetAllAsync(cancellationToken));
    }

    /// <summary>
    /// Creates a provider.
    /// </summary>
    [Right(SecurityConstants.Edit), HttpPost("create-provider")]
    [ProducesResponseType<LlmProviderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LlmProviderDto>> Create([FromBody] SaveLlmProviderRequest request, CancellationToken cancellationToken)
    {
        return Ok(await providerService.CreateAsync(request, cancellationToken));
    }

    /// <summary>
    /// Updates a provider. Send an empty <c>apiKey</c> to keep the stored key.
    /// </summary>
    [Right(SecurityConstants.Edit), HttpPut("update-provider/{id:int}")]
    [ProducesResponseType<LlmProviderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LlmProviderDto>> Update(int id, [FromBody] SaveLlmProviderRequest request, CancellationToken cancellationToken)
    {
        return Ok(await providerService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Deletes a provider.
    /// </summary>
    [Right(SecurityConstants.Delete), HttpDelete("delete-provider/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await providerService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Checks connectivity to the provider by listing its models with the stored API key.
    /// </summary>
    [Right(SecurityConstants.Read), HttpPost("test-provider/{id:int}")]
    [ProducesResponseType<TestLlmProviderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestLlmProviderResponse>> Test(int id, CancellationToken cancellationToken)
    {
        return Ok(await providerService.TestAsync(id, cancellationToken));
    }

    /// <summary>
    /// Lists models available at a provider endpoint. Uses the key from the request or, when it is empty,
    /// the key stored for <c>providerId</c>.
    /// </summary>
    [Right(SecurityConstants.Read), HttpPost("get-provider-models")]
    [ProducesResponseType<List<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<string>>> GetProviderModels([FromBody] GetLlmProviderModelsRequest request, CancellationToken cancellationToken)
    {
        return Ok(await providerService.GetModelsAsync(request, cancellationToken));
    }

    /// <inheritdoc />
    public override List<SecurityResponse> GetModuleRights()
    {
        return
        [
            new SecurityResponse
            {
                Code = SecurityConstants.Read,
                Name = "Read",
                Description = "Can view providers and run connectivity checks",
                Roles = [SecurityConstants.AdminRole]
            },
            new SecurityResponse
            {
                Code = SecurityConstants.Edit,
                Name = "Edit",
                Description = "Can create and edit providers",
                Roles = [SecurityConstants.AdminRole]
            },
            new SecurityResponse
            {
                Code = SecurityConstants.Delete,
                Name = "Delete",
                Description = "Can delete providers",
                Roles = [SecurityConstants.AdminRole]
            }
        ];
    }
}
