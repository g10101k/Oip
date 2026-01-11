using Microsoft.AspNetCore.Mvc;
using Oip.Api.Controllers;
using Oip.Data.Repositories;

namespace Oip.Controllers;

/// <summary>
/// For example
/// </summary>
[ApiController]
[Route("api/dashboard")]
public class DashboardModuleController(ModuleRepository moduleRepository)
    : BaseModuleController<DashboardSettings>(moduleRepository);

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