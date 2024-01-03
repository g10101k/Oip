using Oip.Security.Bl.Helpers;

namespace Oip.Security.Bl.Resources;

public interface IPersistedGrantServiceResources
{
    ResourceMessage PersistedGrantDoesNotExist();

    ResourceMessage PersistedGrantWithSubjectIdDoesNotExist();
}