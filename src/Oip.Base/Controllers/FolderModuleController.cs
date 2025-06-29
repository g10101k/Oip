using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Repositories;
using Oip.Base.Properties;

namespace Oip.Base.Controllers;

/// <summary>
/// Folder module
/// </summary>
[ApiController]
[Route("api/folder-module")]
[ApiExplorerSettings(GroupName = "base")]
public class FolderModuleController : BaseModuleController<FolderModuleSettings>
{
    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="moduleRepository"></param>
    public FolderModuleController(ModuleRepository moduleRepository) : base(moduleRepository)
    {
    }
    
    /// <inheritdoc />
    public override List<SecurityResponse> GetModuleRights()
    {
        return new()
        {
            new()
            {
                Code = Resources.FolderModuleController_GetModuleRights_read,
                Name = Resources.FolderModuleController_GetModuleRights_read,
                Description = Resources.FolderModuleController_GetModuleRights_Can_view_this_module, 
                Roles = ["admin"]
            },
        };
    }
}

/// <summary>
/// Module settings.
/// </summary>
public class FolderModuleSettings
{
    /// <summary>
    /// HTML content for the module.
    /// </summary>
    public string Html { get; set; } = string.Empty;
}