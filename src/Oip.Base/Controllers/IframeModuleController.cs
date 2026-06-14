using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Api.Controllers.Api;
using Oip.Base.Properties;
using Oip.Data.Constants;
using Oip.Data.Repositories;

namespace Oip.Api.Controllers;

[ApiController]
[Authorize, Route("api/iframe-module")]
[ApiExplorerSettings(GroupName = "base")]
public class IframeModuleController(ModuleRepository moduleRepository)
    : BaseModuleController<IframeModuleSettings>(moduleRepository)
{
    public override List<SecurityResponse> GetModuleRights()
    {
        return
        [
            new SecurityResponse
            {
                Code = SecurityConstants.Read,
                Name = Resources.ModuleModuleController_GetModuleRights_Read,
                Description = Resources.FolderModuleController_GetModuleRights_Can_view_this_module,
                Roles = [SecurityConstants.AdminRole]
            }
        ];
    }
}

public class IframeModuleSettings
{
    public string? Url { get; set; }
}
