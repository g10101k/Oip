using Microsoft.AspNetCore.Mvc;
using Oip.Controllers.Api;
using Oip.Data.Repositories;

namespace Oip.Controllers;

/// <summary>
/// For example
/// </summary>
[ApiController]
[Route("api/dashboard")]
public class DashboardFeatureController : BaseFeatureController<DashboardSettings>
{
    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="featureRepository"></param>
    public DashboardFeatureController(FeatureRepository featureRepository) : base(featureRepository)
    {
    }


    /// <inheritdoc />
    public override List<SecurityResponse> GetFeatureRights()
    {
        return new()
        {
            new() { Code = "read", Name = "Read", Description = "Can view this feature", Roles = ["admin"] },
        };
    }
}

/// <summary>
/// Settings
/// </summary>
public class DashboardSettings
{
    /// <summary>
    /// Just for example
    /// </summary>
    public string Nothing { get; set; } = "default value";
}