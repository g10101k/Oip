using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Api.Controllers;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Repositories;
using Oip.Base.Exceptions;
using Oip.Rtds.Data.Dtos;
using Oip.Rtds.Data.Repositories;
using Resources = Oip.Rtds.Properties.Resources;

namespace Oip.Rtds.Controllers;

/// <summary>
/// Controller for managing tags in the Tag Management module.
/// </summary>
/// <remarks>
/// Provides endpoints to add new tags and retrieve tags using a name-based filter.  
/// Also defines access rights required to use this module.
/// </remarks>
[ApiController]
[Route("api/tag-management")]
public class TagManagementModuleController(TagRepository tagRepository, ModuleRepository moduleRepository)
    : BaseModuleController<object>(moduleRepository)
{
    /// <summary>
    /// Adds a new tag.
    /// </summary>
    /// <param name="createTag">Tag entity to be added.</param>
    /// <returns>HTTP 200 OK on success.</returns>
    [HttpPost("add-tag")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<OipException>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<OipException>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddTag(CreateTagDto createTag)
    {
        await tagRepository.AddTag(createTag);
        return Ok();
    }

    /// <summary>
    /// Retrieves tags that match a given name filter.
    /// </summary>
    /// <param name="filter">Name filter to search tags by.</param>
    /// <returns>A list of matching tags.</returns>
    [HttpGet("get-tags-by-filter")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType<List<TagDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<OipException>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<OipException>(StatusCodes.Status500InternalServerError)]
    public ActionResult<List<TagDto>> GetTagsByFilter(string filter)
    {
        return Ok(tagRepository.GetTagsByFilter(filter));
    }

    /// <summary>
    /// Edits an existing tag.
    /// </summary>
    /// <param name="createTag">The tag object containing updated information.</param>
    /// <returns>An IActionResult indicating success.</returns>
    [HttpPost("edit-tag")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<OipException>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<OipException>(StatusCodes.Status500InternalServerError)]
    public IActionResult EditTag(CreateTagDto createTag)
    {
        tagRepository.EditTag(createTag);
        return Ok();
    }
}