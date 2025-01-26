using Oip.Controllers.Api;

namespace Oip.Controllers;

#pragma warning disable CS1591
public interface IModuleControllerSecurity
{
    /// <summary>
    /// Get rights 
    /// </summary>
    /// <returns></returns>
    List<SecurityResponse> GetModuleRights();
}