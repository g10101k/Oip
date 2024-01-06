using Oip.Security.Api.ExceptionHandling;

namespace Oip.Security.Api.Resources;

public class ApiErrorResources : IApiErrorResources
{
    public virtual ApiError CannotSetId()
    {
        return new ApiError
        {
            Code = nameof(CannotSetId),
            Description = ApiErrorResource.CannotSetId
        };
    }
}