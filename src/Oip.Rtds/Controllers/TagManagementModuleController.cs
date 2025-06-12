using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Repositories;
using Oip.Rtds.Data.Contexts;
using Oip.Rtds.Data.Entities;
using Resources = Oip.Rts.Properties.Resources;

namespace Oip.Rts.Controllers;

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
    private readonly RtdsContext _rtdsContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagManagementModuleController"/> class.
    /// </summary>
    /// <remarks>
    /// The controller depends on an application context and a module repository 
    /// for database access and modular behavior, respectively.
    /// </remarks>
    /// <param name="rtdsContext">Application database context for tag operations.</param>
    /// <param name="moduleRepository">Base module repository for module-level operations.</param>
    public TagManagementModuleController(RtdsContext rtdsContext, ModuleRepository moduleRepository)
        : base(moduleRepository)
    {
        _rtdsContext = rtdsContext;
    }

    /// <summary>
    /// Adds a new tag.
    /// </summary>
    /// <remarks>
    /// Accepts a tag entity from the request body and stores it in the database.  
    /// Returns HTTP 200 on success.
    /// </remarks>
    /// <param name="tag">Tag entity to be added.</param>
    /// <returns>HTTP 200 OK on success.</returns>
    [HttpPost("add-tag")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult AddTag(TagEntity tag)
    {
        _rtdsContext.AddTag(tag);
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
    [ProducesResponseType<List<TagEntity>>(StatusCodes.Status200OK)]
    public IActionResult GetTagsByFilter(string filter)
    {
        return Ok(_rtdsContext.GetTagsByFilter(filter));
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
                Code = SecurityConstants.ReadRight,
                Name = Resources.TagController_GetModuleRights_Read,
                Description = Resources.TagController_GetModuleRights_Cat_view_this_module,
                Roles = [SecurityConstants.AdminRole]
            }
        };
    }
}