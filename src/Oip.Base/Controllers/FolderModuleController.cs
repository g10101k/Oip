using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Repositories;
using Oip.Base.Properties;

namespace Oip.Base.Controllers;

/// <summary>
/// API controller for the Folder module.
/// </summary>
/// <remarks>
/// Provides metadata about the module, including access rights required to interact with it.
/// Inherits from <see cref="BaseModuleController{TSettings}"/> with a generic parameter of <c>object</c>
/// because this module does not require specific configuration.
/// </remarks>
[ApiController]
[Route("api/folder")]
public class FolderModuleController : BaseModuleController<object>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FolderModuleController"/> class.
    /// </summary>
    /// <param name="moduleRepository">The module repository used to retrieve and manage module data.</param>
    public FolderModuleController(ModuleRepository moduleRepository) : base(moduleRepository)
    {
    }

    /// <summary>
    /// Returns a list of rights (permissions) required to access the folder module.
    /// </summary>
    /// <remarks>
    /// This method defines the security model for the folder module. It currently includes only read access,
    /// limited to users with the administrator role.
    /// </remarks>
    /// <returns>
    /// A list of <see cref="SecurityResponse"/> representing the rights associated with the module.
    /// </returns>
    public override List<SecurityResponse> GetModuleRights()
    {
        return new()
        {
            new()
            {
                Code = SecurityConstants.ReadRight,
                Name = Resources.FolderModuleController_GetModuleRights_Read,
                Description = Resources.FolderModuleController_GetModuleRights_Can_view_this_module,
                Roles = [SecurityConstants.AdminRole]
            },
        };
    }
}