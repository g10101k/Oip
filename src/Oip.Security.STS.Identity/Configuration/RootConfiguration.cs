using Oip.Security.Shared.Configuration.Configuration.Identity;
using Oip.Security.STS.Identity.Configuration.Interfaces;

namespace Oip.Security.STS.Identity.Configuration;

public class RootConfiguration : IRootConfiguration
{
    public AdminConfiguration AdminConfiguration { get; } = new();
    public RegisterConfiguration RegisterConfiguration { get; } = new();
}