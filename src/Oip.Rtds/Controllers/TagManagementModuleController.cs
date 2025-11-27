using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Constants;
using Oip.Base.Controllers;
using Oip.Base.Controllers.Api;
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
    /// <remarks>
    /// The controller depends on an application context and a module repository 
    /// for database access and modular behavior, respectively.
    /// </remarks>
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
    /// <remarks>
    /// Accepts a tag entity from the request body and stores it in the database.  
    /// Returns HTTP 200 on success.
    /// </remarks>
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
    /// <remarks>
    /// Returns a list of tags whose names match the provided filter string.  
    /// This is useful for searching or filtering tags by partial name.
    /// </remarks>
    /// <param name="filter">Name filter to search tags by.</param>
    /// <returns>A list of matching tags.</returns>
    [HttpGet("get-tags-by-filter")]
    [Authorize]
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

    /// <summary>
    /// Returns the security rights required for accessing this module.
    /// </summary>
    /// <remarks>
    /// This method defines the access control rules for the Tag Management module.  
    /// It lists the roles and permissions necessary to interact with the module via the UI or API.
    /// </remarks>
    /// <returns>A list of security rights.</returns>
    public override List<SecurityResponse> GetModuleRights()
    {
        return new()
        {
            new()
            {
                Code = SecurityConstants.Read,
                Name = Resources.TagController_GetModuleRights_Read,
                Description = Resources.TagController_GetModuleRights_Cat_view_this_module,
                Roles = [SecurityConstants.AdminRole]
            }
        };
    }
}