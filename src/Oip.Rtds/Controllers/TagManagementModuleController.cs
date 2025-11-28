using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Api.Controllers;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Repositories;
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
public class TagManagementModuleController : BaseModuleController<object>
{
    private readonly TagRepository _tagRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagManagementModuleController"/> class.
    /// </summary>
    /// <param name="tagRepository">Application database context for tag operations.</param>
    /// <param name="moduleRepository">Base module repository for module-level operations.</param>
    public TagManagementModuleController(TagRepository tagRepository, ModuleRepository moduleRepository)
        : base(moduleRepository)
    {
        _tagRepository = tagRepository;
    }

    /// <summary>
    /// Adds a new tag.
    /// </summary>
    /// <param name="createTag">Tag entity to be added.</param>
    /// <returns>HTTP 200 OK on success.</returns>
    [HttpPost("add-tag")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AddTag(CreateTagDto createTag)
    {
        await _tagRepository.AddTag(createTag);
        return Ok();
    }

    /// <summary>
    /// Retrieves tags that match a given name filter.
    /// </summary>
    /// <param name="filter">Name filter to search tags by.</param>
    /// <returns>A list of matching tags.</returns>
    [HttpGet("get-tags-by-filter")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public ActionResult<List<TagDto>> GetTagsByFilter(string filter)
    {
        return Ok(_tagRepository.GetTagsByFilter(filter));
    }

    /// <summary>
    /// Edits an existing tag.
    /// </summary>
    /// <param name="createTag">The tag object containing updated information.</param>
    /// <returns>An IActionResult indicating success.</returns>
    [HttpPost("edit-tag")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    public IActionResult EditTag(CreateTagDto createTag)
    {
        _tagRepository.EditTag(createTag);
        return Ok();
    }
}