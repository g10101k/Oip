using Oip.Security.Bl.Helpers;

namespace Oip.Security.Bl.Resources;

public interface IApiResourceServiceResources
{
    ResourceMessage ApiResourceDoesNotExist();
    ResourceMessage ApiResourceExistsValue();
    ResourceMessage ApiResourceExistsKey();
    ResourceMessage ApiSecretDoesNotExist();
    ResourceMessage ApiResourcePropertyDoesNotExist();
    ResourceMessage ApiResourcePropertyExistsKey();
    ResourceMessage ApiResourcePropertyExistsValue();
}