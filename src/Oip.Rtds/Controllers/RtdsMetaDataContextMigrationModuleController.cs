using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers;
using Oip.Base.Data.Repositories;
using Oip.Rtds.Data.Contexts;

namespace Oip.Rtds.Controllers;

/// <summary>
/// Controller for managing database migrations of the RTDS metadata context
/// </summary>
[ApiController]
[Route("api/rtds-meta-data-context-migration-module")]
public class RtdsMetaDataContextMigrationModuleController(ModuleRepository repository, RtdsMetaContext dbContext)
    : BaseDbMigrationController<object>(repository, dbContext);