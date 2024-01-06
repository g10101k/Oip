using System.Collections.Generic;
using Oip.Security.Dal.Configuration.Configuration.Identity;

namespace Oip.Security.Dal.Configuration;

public class IdentityData
{
    public List<Role> Roles { get; set; } = new();
    public List<User> Users { get; set; } = new();
}