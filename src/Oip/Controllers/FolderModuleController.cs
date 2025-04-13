using Microsoft.AspNetCore.Mvc;
using Oip.Controllers.Api;
using Oip.Data.Repositories;

namespace Oip.Controllers;

/// <summary>
/// For example
/// </summary>
[ApiController]
[Route("api/folder")]
public class FolderModuleController : BaseModuleController<FolderMoSettings>
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
            new() { Code = "read", Name = "Read", Description = "Can view this module", Roles = ["admin"] },
        };
    }
}

/// <summary>
/// Settings
/// </summary>
public class FolderMoSettings
{
    /// <summary>
    /// Just for example
    /// </summary>
    public string Nothing { get; set; } = "default value";
}
