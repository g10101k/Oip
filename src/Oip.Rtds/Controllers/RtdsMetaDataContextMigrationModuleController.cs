using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Repositories;
using Oip.Rtds.Data.Contexts;

namespace Oip.Rts.Controllers;

/// <summary>
/// Controller for managing database migrations of the RTDS metadata context
/// </summary>
[ApiController]
[Route("api/rtds-meta-data-context-migration-module")]
public class RtdsMetaDataContextMigrationModuleController : BaseDbMigrationController<object>
{
    /// <summary>
    /// Initializes a new instance of the migration controller
    /// </summary>
    /// <param name="repository">Module repository instance</param>
    /// <param name="dbContext">RTDS metadata database context</param>
    public RtdsMetaDataContextMigrationModuleController(ModuleRepository repository, RtdsMetaContext dbContext)
        : base(repository, dbContext)
    {
    }

    /// <summary>
    /// Gets the list of access rights for the migration module
    /// </summary>
    /// <returns>List of SecurityResponse objects with access rights</returns>
    /// <remarks>
    /// By default, returns read permission available only for admin role
    /// </remarks>
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