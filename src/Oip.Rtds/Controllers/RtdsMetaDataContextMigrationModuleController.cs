using Microsoft.AspNetCore.Mvc;
using Oip.Api.Controllers;
using Oip.Api.Controllers.Api;
using Oip.Data.Repositories;
using Oip.Rtds.Data.Contexts;
using Oip.Rtds.Properties;

namespace Oip.Rtds.Controllers;

/// <summary>
/// Controller for managing database migrations of the RTDS metadata context
/// </summary>
[ApiController]
[Route("api/rtds-meta-data-context-migration-module")]
public class RtdsMetaDataContextMigrationModuleController(ModuleRepository repository, RtdsMetaContext dbContext)
    : BaseDbMigrationController<object>(repository, dbContext);