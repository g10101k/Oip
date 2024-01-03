using Oip.Security.Bl.Helpers;

namespace Oip.Security.Bl.Resources;

public class PersistedGrantAspNetIdentityServiceResources : IPersistedGrantAspNetIdentityServiceResources
{
    public virtual ResourceMessage PersistedGrantDoesNotExist()
    {
        return new ResourceMessage
        {
            Code = nameof(PersistedGrantDoesNotExist),
            Description = PersistedGrantServiceResource.PersistedGrantDoesNotExist
        };
    }

    public virtual ResourceMessage PersistedGrantWithSubjectIdDoesNotExist()
    {
        return new ResourceMessage
        {
            Code = nameof(PersistedGrantWithSubjectIdDoesNotExist),
            Description = PersistedGrantServiceResource.PersistedGrantWithSubjectIdDoesNotExist
        };
    }
}