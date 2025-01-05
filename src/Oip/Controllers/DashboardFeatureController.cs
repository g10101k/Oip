using Microsoft.AspNetCore.Mvc;
using Oip.Controllers.Api;
using Oip.Data.Repositories;

#pragma warning disable CS1591

namespace Oip.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardFeatureController : BaseFeatureController
{
    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="featureRepository"></param>
    public DashboardFeatureController(FeatureRepository featureRepository) : base(featureRepository)
    {
    }


    public override List<SecurityResponse> GetFeatureRights()
    {
        return new()
        {
            new() { Code = "read", Name = "Read", Description = "Can view this feature", Roles = ["admin"] },
        };
    }
}