using System.Collections.Generic;
using IdentityServer4.Models;
using Client = Oip.Security.Dal.Configuration.Configuration.IdentityServer.Client;

namespace Oip.Security.Dal.Configuration;

public class IdentityServerData
{
    public List<Client> Clients { get; } = new();
    public List<IdentityResource> IdentityResources { get; } = new();
    public List<ApiResource> ApiResources { get; } = new();
    public List<ApiScope> ApiScopes { get; } = new();
}