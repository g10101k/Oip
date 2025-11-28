using Microsoft.AspNetCore.Mvc;
using Oip.Base.Constants;
using Oip.Base.Controllers;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Repositories;
using Oip.Properties;

namespace Oip.Controllers;

/// <summary>
/// For example
/// </summary>
[ApiController]
[Route("api/dashboard")]
public class DashboardModuleController : BaseModuleController<DashboardSettings>
{
    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="moduleRepository"></param>
    public DashboardModuleController(ModuleRepository moduleRepository) : base(moduleRepository)
    {
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