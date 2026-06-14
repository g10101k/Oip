using Oip.Api.Controllers.Api;

namespace Oip.Api.Controllers;

#pragma warning disable CS1591
public interface IModuleControllerSecurity
{
    /// <summary>
    /// Get rights 
    /// </summary>
    /// <returns></returns>
    List<SecurityResponse> GetModuleRights();
}