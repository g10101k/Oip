using Microsoft.AspNetCore.Mvc;
using Oip.Api.Controllers;
using Oip.Api.Controllers.Api;
using Oip.Data.Constants;
using Oip.Data.Repositories;
using Oip.Properties;
namespace Oip.Controllers;

/// <summary>
/// For example
/// </summary>
[ApiController]
[Route("api/dashboard")]
public class DashboardModuleController(ModuleRepository moduleRepository)
    : BaseModuleController<DashboardSettings>(moduleRepository)
{
    /// <inheritdoc />
    public override List<SecurityResponse> GetModuleRights()
    {
        return
        [
            new SecurityResponse
            {
                Code = SecurityConstants.Read,
                Name = Resources.DashboardModuleController_GetModuleRights_Read,
                Description = Resources.DashboardModuleController_GetModuleRights_Can_view_this_module,
                Roles = [SecurityConstants.AdminRole]
            }
        ];
    }
}

/// <summary>
/// Settings
/// </summary>
public class DashboardSettings
{
    /// <summary>
    /// Just, for example
    /// </summary>
    public string Nothing { get; set; } = "default value";
}