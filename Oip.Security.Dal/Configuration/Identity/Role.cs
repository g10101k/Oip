using System.Collections.Generic;

namespace Oip.Security.Dal.Configuration.Configuration.Identity;

public class Role
{
    public string Name { get; set; }
    public List<Claim> Claims { get; set; } = new();
}