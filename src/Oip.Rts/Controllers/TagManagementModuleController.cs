using Microsoft.AspNetCore.Mvc;
using Oip.Base.Api;
using Oip.Base.Controllers;
using Oip.Base.Data.Repositories;
using Oip.Rts.Base.Contexts;
using Resources = Oip.Rts.Properties.Resources;

namespace Oip.Rts.Controllers;

/// <summary>
/// Module controller example
/// </summary>
[ApiController]
[Route("api/tag-management-module")]
public class TagManagementModuleController : BaseModuleController<object>
{
    private readonly RtsContext _rtsContext;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="rtsContext"></param>
    /// <param name="moduleRepository"></param>
    public TagManagementModuleController(RtsContext rtsContext, ModuleRepository moduleRepository) : base(moduleRepository)
    {
        _rtsContext = rtsContext;
    }

    /// <summary>
    /// Add tag
    /// </summary>
    /// <returns></returns>
    [HttpPost("add-tag")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult AddTag(TagEntity tag)
    {
        _rtsContext.AddTag(tag);
        return Ok();
    }

    /// <summary>
    /// Get tag by name filter
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-tags-by-filter")]
    [ProducesResponseType<List<TagEntity>>(StatusCodes.Status200OK)]
    public IActionResult GetTagsByFilter(string filter)
    {
        return Ok(_rtsContext.GetTagsByFilter(filter));
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public override List<SecurityResponse> GetModuleRights()
    {
        return new()
        {
            new()
            {
                Code = "read",
                Name = Resources.TagController_GetModuleRights_Read,
                Description = Resources.TagController_GetModuleRights_Cat_view_this_module,
                Roles = ["admin"]
            }
        };
    }
}



public class CreateTagRequest
{
}