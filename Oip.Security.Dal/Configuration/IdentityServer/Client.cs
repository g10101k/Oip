using System.Collections.Generic;
using Oip.Security.Dal.Configuration.Configuration.Identity;

namespace Oip.Security.Dal.Configuration.Configuration.IdentityServer;

public class Client : IdentityServer4.Models.Client
{
    public List<Claim> ClientClaims { get; set; } = new();
}