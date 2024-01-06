using System.Collections.Generic;

namespace Oip.Security.Bl.Dtos.Identity.Interfaces;

public interface IUserProvidersDto : IUserProviderDto
{
    List<IUserProviderDto> Providers { get; }
}