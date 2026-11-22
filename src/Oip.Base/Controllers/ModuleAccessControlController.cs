using Microsoft.AspNetCore.Mvc;
using Oip.Base.Security.Objects;

namespace Oip.Base.Controllers;

/// <summary>
/// Access control of the module instances and menu folders. The client calls it through ObjectRightsService of
/// oip-common, so no generated client is published.
/// </summary>
[Route("api/module-access-control")]
[ApiExplorerSettings(IgnoreApi = true)]
public class ModuleAccessControlController(IObjectAccessService access, ISubjectDirectory subjectDirectory)
    : BaseAccessControlController(access, subjectDirectory);
