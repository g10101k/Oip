using Oip.Security.Bl.Helpers;

namespace Oip.Security.Bl.Resources;

public interface IClientServiceResources
{
    ResourceMessage ClientClaimDoesNotExist();

    ResourceMessage ClientDoesNotExist();

    ResourceMessage ClientExistsKey();

    ResourceMessage ClientExistsValue();

    ResourceMessage ClientPropertyDoesNotExist();

    ResourceMessage ClientSecretDoesNotExist();
}