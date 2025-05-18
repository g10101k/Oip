using Oip.Base.Api;

namespace Oip.Base.Controllers;

#pragma warning disable CS1591
public interface IModuleControllerSecurity
{
    /// <summary>
    /// Get rights 
    /// </summary>
    /// <returns></returns>
    List<SecurityResponse> GetModuleRights();
}