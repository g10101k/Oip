using Microsoft.AspNetCore.Mvc;
using Oip.Base.Constants;
using Oip.Base.Controllers;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Repositories;
using Oip.Example.Data.Contexts;
using Oip.Properties;

namespace Oip.Controllers;

/// <summary>
/// Db migration controller
/// </summary>
[ApiController]
[Route("api/db-migration")]
public class DataContextMigrationModuleController : BaseDbMigrationController<object>
{
    /// <inheritdoc />
    public DataContextMigrationModuleController(ModuleRepository repository, ExampleDataContext dbContext)
        : base(repository, dbContext)
    {
    }

    /// <inheritdoc />
    public override List<SecurityResponse> GetModuleRights()
    {
        return new()
        {
            new()
            {
                Code = SecurityConstants.Delete,
                Name = Resources.DataContextMigrationController_GetModuleRights_Read,
                Description = Resources.DataContextMigrationController_GetModuleRights_Can_this_module,
                Roles = [SecurityConstants.AdminRole]
            },
        };
    }
}