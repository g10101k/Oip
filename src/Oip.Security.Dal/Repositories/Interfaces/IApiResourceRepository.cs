using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using Oip.Security.Dal.Common;

namespace Oip.Security.Dal.Repositories.Interfaces;

public interface IApiResourceRepository
{
    bool AutoSaveChanges { get; set; }
    Task<PagedList<ApiResource>> GetApiResourcesAsync(string search, int page = 1, int pageSize = 10);

    Task<ApiResource> GetApiResourceAsync(int apiResourceId);

    Task<PagedList<ApiResourceProperty>> GetApiResourcePropertiesAsync(int apiResourceId, int page = 1,
        int pageSize = 10);

    Task<ApiResourceProperty> GetApiResourcePropertyAsync(int apiResourcePropertyId);

    Task<int> AddApiResourcePropertyAsync(int apiResourceId, ApiResourceProperty apiResourceProperty);

    Task<int> DeleteApiResourcePropertyAsync(ApiResourceProperty apiResourceProperty);

    Task<bool> CanInsertApiResourcePropertyAsync(ApiResourceProperty apiResourceProperty);

    Task<int> AddApiResourceAsync(ApiResource apiResource);

    Task<int> UpdateApiResourceAsync(ApiResource apiResource);

    Task<int> DeleteApiResourceAsync(ApiResource apiResource);

    Task<bool> CanInsertApiResourceAsync(ApiResource apiResource);

    Task<PagedList<ApiResourceSecret>> GetApiSecretsAsync(int apiResourceId, int page = 1, int pageSize = 10);

    Task<int> AddApiSecretAsync(int apiResourceId, ApiResourceSecret apiSecret);

    Task<ApiResourceSecret> GetApiSecretAsync(int apiSecretId);

    Task<int> DeleteApiSecretAsync(ApiResourceSecret apiSecret);

    Task<int> SaveAllChangesAsync();

    Task<string> GetApiResourceNameAsync(int apiResourceId);
}