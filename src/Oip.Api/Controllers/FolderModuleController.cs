using Microsoft.AspNetCore.Mvc;
using Oip.Api.Controllers.Api;
using Oip.Api.Properties;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Repositories;

namespace Oip.Api.Controllers;

/// <summary>
/// API controller for the Folder module.
/// </summary>
[ApiController]
[Route("api/folder-module")]
[ApiExplorerSettings(GroupName = "base")]
public class FolderModuleController(ModuleRepository moduleRepository)
    : BaseModuleController<FolderModuleSettings>(moduleRepository)
{
    /// <summary>
    /// Returns a list of rights (permissions) required to access the folder module.
    /// </summary>
    /// <returns>
    /// A list of <see cref="SecurityResponse"/> representing the rights associated with the module.
    /// </returns>
    public override List<SecurityResponse> GetModuleRights()
    {
        return new()
        {
            new()
            {
                Code = SecurityConstants.Read,
                Name = Resources.ModuleModuleController_GetModuleRights_Read,
                Description = Resources.FolderModuleController_GetModuleRights_Can_view_this_module,
                Roles = [SecurityConstants.AdminRole]
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