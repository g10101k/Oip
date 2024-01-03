using Oip.Security.Bl.Helpers;

namespace Oip.Security.Bl.Resources;

public interface IApiScopeServiceResources
{
    ResourceMessage ApiScopeDoesNotExist();
    ResourceMessage ApiScopeExistsValue();
    ResourceMessage ApiScopeExistsKey();
    ResourceMessage ApiScopePropertyExistsValue();
    ResourceMessage ApiScopePropertyDoesNotExist();
    ResourceMessage ApiScopePropertyExistsKey();
}