using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers;
using Oip.Base.Security.Objects;

namespace Oip.Controllers;

/// <summary>
/// Access control of the demo customers and their categories.
/// </summary>
[Route("api/customer-access-control")]
public class CustomerAccessControlController(IObjectAccessService access, ISubjectDirectory subjectDirectory)
    : BaseAccessControlController(access, subjectDirectory);
