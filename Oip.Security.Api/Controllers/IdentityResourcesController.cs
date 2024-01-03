using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Security.Api.Configuration.Constants;
using Oip.Security.Api.Dtos.IdentityResources;
using Oip.Security.Api.ExceptionHandling;
using Oip.Security.Api.Mappers;
using Oip.Security.Api.Resources;
using Oip.Security.Bl.Dtos.Configuration;
using Oip.Security.Bl.Services.Interfaces;

namespace Oip.Security.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[TypeFilter(typeof(ControllerExceptionFilterAttribute))]
[Produces("application/json", "application/problem+json")]
[Authorize(Policy = AuthorizationConsts.AdministrationPolicy)]
public class IdentityResourcesController : ControllerBase
{
    private readonly IApiErrorResources _errorResources;
    private readonly IIdentityResourceService _identityResourceService;

    public IdentityResourcesController(IIdentityResourceService identityResourceService,
        IApiErrorResources errorResources)
    {
        _identityResourceService = identityResourceService;
        _errorResources = errorResources;
    }

    [HttpGet]
    public async Task<ActionResult<IdentityResourcesApiDto>> Get(string searchText, int page = 1, int pageSize = 10)
    {
        var identityResourcesDto = await _identityResourceService.GetIdentityResourcesAsync(searchText, page, pageSize);
        var identityResourcesApiDto = identityResourcesDto.ToIdentityResourceApiModel<IdentityResourcesApiDto>();

        return Ok(identityResourcesApiDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IdentityResourceApiDto>> Get(int id)
    {
        var identityResourceDto = await _identityResourceService.GetIdentityResourceAsync(id);
        var identityResourceApiModel = identityResourceDto.ToIdentityResourceApiModel<IdentityResourceApiDto>();

        return Ok(identityResourceApiModel);
    }

    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Post([FromBody] IdentityResourceApiDto identityResourceApi)
    {
        var identityResourceDto = identityResourceApi.ToIdentityResourceApiModel<IdentityResourceDto>();

        if (!identityResourceDto.Id.Equals(default)) return BadRequest(_errorResources.CannotSetId());

        var id = await _identityResourceService.AddIdentityResourceAsync(identityResourceDto);
        identityResourceApi.Id = id;

        return CreatedAtAction(nameof(Get), new { id }, identityResourceApi);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] IdentityResourceApiDto identityResourceApi)
    {
        var identityResource = identityResourceApi.ToIdentityResourceApiModel<IdentityResourceDto>();

        await _identityResourceService.GetIdentityResourceAsync(identityResource.Id);
        await _identityResourceService.UpdateIdentityResourceAsync(identityResource);

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var identityResource = new IdentityResourceDto { Id = id };

        await _identityResourceService.GetIdentityResourceAsync(identityResource.Id);
        await _identityResourceService.DeleteIdentityResourceAsync(identityResource);

        return Ok();
    }

    [HttpGet("{id}/Properties")]
    public async Task<ActionResult<IdentityResourcePropertiesApiDto>> GetProperties(int id, int page = 1,
        int pageSize = 10)
    {
        var identityResourcePropertiesDto =
            await _identityResourceService.GetIdentityResourcePropertiesAsync(id, page, pageSize);
        var identityResourcePropertiesApiDto =
            identityResourcePropertiesDto.ToIdentityResourceApiModel<IdentityResourcePropertiesApiDto>();

        return Ok(identityResourcePropertiesApiDto);
    }

    [HttpGet("Properties/{propertyId}")]
    public async Task<ActionResult<IdentityResourcePropertyApiDto>> GetProperty(int propertyId)
    {
        var identityResourcePropertiesDto = await _identityResourceService.GetIdentityResourcePropertyAsync(propertyId);
        var identityResourcePropertyApiDto =
            identityResourcePropertiesDto.ToIdentityResourceApiModel<IdentityResourcePropertyApiDto>();

        return Ok(identityResourcePropertyApiDto);
    }

    [HttpPost("{id}/Properties")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> PostProperty(int id,
        [FromBody] IdentityResourcePropertyApiDto identityResourcePropertyApi)
    {
        var identityResourcePropertiesDto =
            identityResourcePropertyApi.ToIdentityResourceApiModel<IdentityResourcePropertiesDto>();
        identityResourcePropertiesDto.IdentityResourceId = id;

        if (!identityResourcePropertiesDto.IdentityResourcePropertyId.Equals(default))
            return BadRequest(_errorResources.CannotSetId());

        var propertyId = await _identityResourceService.AddIdentityResourcePropertyAsync(identityResourcePropertiesDto);
        identityResourcePropertyApi.Id = propertyId;

        return CreatedAtAction(nameof(GetProperty), new { propertyId }, identityResourcePropertyApi);
    }

    [HttpDelete("Properties/{propertyId}")]
    public async Task<IActionResult> DeleteProperty(int propertyId)
    {
        var identityResourceProperty = new IdentityResourcePropertiesDto { IdentityResourcePropertyId = propertyId };

        await _identityResourceService.GetIdentityResourcePropertyAsync(identityResourceProperty
            .IdentityResourcePropertyId);
        await _identityResourceService.DeleteIdentityResourcePropertyAsync(identityResourceProperty);

        return Ok();
    }
}