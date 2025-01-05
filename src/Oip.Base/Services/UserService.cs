using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Oip.Base.Services;

public class UserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// .ctor
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    public UserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Get user e-mail
    /// </summary>
    /// <returns></returns>
    public string? GetUserEmail()
    {
        if (_httpContextAccessor.HttpContext == null)
            return null;

        var searchResult = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
        if (searchResult is { } claim)
        {
            return claim.Value;
        }

        return null;
    }

    /// <summary>
    /// Get user roles from 
    /// </summary>
    /// <returns></returns>
    public List<string> GetUserRoles()
    {
        if (_httpContextAccessor.HttpContext == null)
            return [];

        return _httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
    }
}