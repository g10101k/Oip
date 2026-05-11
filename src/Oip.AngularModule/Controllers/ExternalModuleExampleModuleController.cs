using Microsoft.AspNetCore.Mvc;
using Oip.Api.Controllers;
using Oip.Api.Controllers.Api;
using Oip.Data.Constants;
using Oip.Data.Repositories;

namespace Oip.AngularModule.Controllers;

/// <summary>
/// Controller for the ExternalModuleExample module.
/// </summary>
[ApiController]
[Route("api/external-module-example-module")]
public class ExternalModuleExampleModuleController(ModuleRepository moduleRepository)
    : BaseModuleController<ExternalModuleExampleModuleSettings>(moduleRepository)
{
    /// <inheritdoc />
    public override List<SecurityResponse> GetModuleRights()
    {
        return
        [
            new SecurityResponse
            {
                Code = SecurityConstants.Read,
                Name = "Read",
                Description = "Can view this module",
                Roles = [SecurityConstants.AdminRole]
            }
        ];
    }
}

/// <summary>
/// Settings for the ExternalModuleExample module.
/// </summary>
public class ExternalModuleExampleModuleSettings
{
}