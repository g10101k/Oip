using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers;
using Oip.Base.Security.Objects;

namespace Oip.Controllers;

/// <summary>
/// Access control of the demo customers and their categories. The client calls it through ObjectRightsService of
/// oip-common, so no generated client is published.
/// </summary>
[Route("api/customer-access-control")]
[ApiExplorerSettings(IgnoreApi = true)]
public class CustomerAccessControlController(IObjectAccessService access, ISubjectDirectory subjectDirectory)
    : BaseAccessControlController(access, subjectDirectory);
