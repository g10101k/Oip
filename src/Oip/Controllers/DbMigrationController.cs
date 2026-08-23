using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers;
using Oip.Base.Data.Contexts;
using Oip.Base.Data.Repositories;

namespace Oip.Controllers;

/// <summary>
/// Controller for managing database migrations of the OIP module context
/// </summary>
[ApiController]
[Route("api/db-migration")]
public class DbMigrationController(ModuleRepository repository, OipModuleContext dbContext)
    : BaseDbMigrationController<object>(repository, dbContext);
