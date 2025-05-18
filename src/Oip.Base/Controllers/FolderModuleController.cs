using Microsoft.AspNetCore.Mvc;
using Oip.Base.Api;
using Oip.Base.Data.Repositories;

namespace Oip.Base.Controllers;

/// <summary>
/// Folder module
/// </summary>
[ApiController]
[Route("api/folder")]
public class FolderModuleController : BaseModuleController<object>
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