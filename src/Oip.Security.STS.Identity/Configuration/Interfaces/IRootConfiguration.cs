using Oip.Security.Shared.Configuration.Configuration.Identity;

namespace Oip.Security.STS.Identity.Configuration.Interfaces;

public interface IRootConfiguration
{
    AdminConfiguration AdminConfiguration { get; }

    RegisterConfiguration RegisterConfiguration { get; }
}