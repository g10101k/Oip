using Oip.Security.Bl.Helpers;

namespace Oip.Security.Bl.Resources;

public interface IIdentityResourceServiceResources
{
    ResourceMessage IdentityResourceDoesNotExist();

    ResourceMessage IdentityResourceExistsKey();

    ResourceMessage IdentityResourceExistsValue();

    ResourceMessage IdentityResourcePropertyDoesNotExist();

    ResourceMessage IdentityResourcePropertyExistsValue();

    ResourceMessage IdentityResourcePropertyExistsKey();
}