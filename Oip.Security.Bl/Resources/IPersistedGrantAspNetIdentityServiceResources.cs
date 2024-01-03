using Oip.Security.Bl.Helpers;

namespace Oip.Security.Bl.Resources;

public interface IPersistedGrantAspNetIdentityServiceResources
{
    ResourceMessage PersistedGrantDoesNotExist();

    ResourceMessage PersistedGrantWithSubjectIdDoesNotExist();
}