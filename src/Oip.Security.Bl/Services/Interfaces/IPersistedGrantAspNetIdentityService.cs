using System.Threading.Tasks;
using Oip.Security.Bl.Dtos.Grant;

namespace Oip.Security.Bl.Services.Interfaces;

public interface IPersistedGrantAspNetIdentityService
{
    Task<PersistedGrantsDto> GetPersistedGrantsByUsersAsync(string search, int page = 1, int pageSize = 10);
    Task<PersistedGrantsDto> GetPersistedGrantsByUserAsync(string subjectId, int page = 1, int pageSize = 10);
    Task<PersistedGrantDto> GetPersistedGrantAsync(string key);
    Task<int> DeletePersistedGrantAsync(string key);
    Task<int> DeletePersistedGrantsAsync(string userId);
}