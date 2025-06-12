using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Repositories;
using Oip.Rtds.Data.Contexts;

namespace Oip.Rts.Controllers;

/// <summary>
/// Db migration controller
/// </summary>
[ApiController]
[Route("api/rtds-meta-data-context-migration-module")]
public class RtdsMetaDataContextMigrationModuleController : BaseDbMigrationController<object>
{
    /// <inheritdoc />
    public RtdsMetaDataContextMigrationModuleController(ModuleRepository repository, RtdsMetaContext dbContext)
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
                Code = SecurityConstants.ReadRight,
                Name = "Read",
                Description = "Can read this module",
                Roles = [SecurityConstants.AdminRole]
            },
        };
    }
}
