using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Oip.Security.Dal.Common;
using Oip.Security.Dal.Extensions.Enums;
using Oip.Security.Dal.Extensions.Extensions;
using Oip.Security.Dal.Interfaces;
using Oip.Security.Dal.Repositories.Interfaces;

namespace Oip.Security.Dal.Repositories;

public class ApiResourceRepository<TDbContext> : IApiResourceRepository
    where TDbContext : DbContext, IAdminConfigurationDbContext
{
    private readonly TDbContext _dbContext;

    public ApiResourceRepository(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public bool AutoSaveChanges { get; set; } = true;

    public virtual async Task<PagedList<ApiResource>> GetApiResourcesAsync(string search, int page = 1,
        int pageSize = 10)
    {
        var pagedList = new PagedList<ApiResource>();
        Expression<Func<ApiResource, bool>> searchCondition = x => x.Name.Contains(search);

        var apiResources = await _dbContext.ApiResources.WhereIf(!string.IsNullOrEmpty(search), searchCondition)
            .PageBy(x => x.Name, page, pageSize).ToListAsync();

        pagedList.Data.AddRange(apiResources);
        pagedList.TotalCount = await _dbContext.ApiResources.WhereIf(!string.IsNullOrEmpty(search), searchCondition)
            .CountAsync();
        pagedList.PageSize = pageSize;

        return pagedList;
    }

    public virtual Task<ApiResource> GetApiResourceAsync(int apiResourceId)
    {
        return _dbContext.ApiResources
            .Include(x => x.UserClaims)
            .Include(x => x.Scopes)
            .Where(x => x.Id == apiResourceId)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    public virtual async Task<PagedList<ApiResourceProperty>> GetApiResourcePropertiesAsync(int apiResourceId,
        int page = 1, int pageSize = 10)
    {
        var pagedList = new PagedList<ApiResourceProperty>();

        var properties = await _dbContext.ApiResourceProperties.Where(x => x.ApiResource.Id == apiResourceId)
            .PageBy(x => x.Id, page, pageSize)
            .ToListAsync();

        pagedList.Data.AddRange(properties);
        pagedList.TotalCount =
            await _dbContext.ApiResourceProperties.Where(x => x.ApiResource.Id == apiResourceId).CountAsync();
        pagedList.PageSize = pageSize;

        return pagedList;
    }

    public virtual Task<ApiResourceProperty> GetApiResourcePropertyAsync(int apiResourcePropertyId)
    {
        return _dbContext.ApiResourceProperties
            .Include(x => x.ApiResource)
            .Where(x => x.Id == apiResourcePropertyId)
            .SingleOrDefaultAsync();
    }

    public virtual async Task<int> AddApiResourcePropertyAsync(int apiResourceId,
        ApiResourceProperty apiResourceProperty)
    {
        var apiResource = await _dbContext.ApiResources.Where(x => x.Id == apiResourceId).SingleOrDefaultAsync();

        apiResourceProperty.ApiResource = apiResource;
        await _dbContext.ApiResourceProperties.AddAsync(apiResourceProperty);

        return await AutoSaveChangesAsync();
    }

    public virtual async Task<int> DeleteApiResourcePropertyAsync(ApiResourceProperty apiResourceProperty)
    {
        var propertyToDelete = await _dbContext.ApiResourceProperties.Where(x => x.Id == apiResourceProperty.Id)
            .SingleOrDefaultAsync();

        _dbContext.ApiResourceProperties.Remove(propertyToDelete);
        return await AutoSaveChangesAsync();
    }

    public virtual async Task<bool> CanInsertApiResourceAsync(ApiResource apiResource)
    {
        if (apiResource.Id == 0)
        {
            var existsWithSameName =
                await _dbContext.ApiResources.Where(x => x.Name == apiResource.Name).SingleOrDefaultAsync();
            return existsWithSameName == null;
        }
        else
        {
            var existsWithSameName = await _dbContext.ApiResources
                .Where(x => x.Name == apiResource.Name && x.Id != apiResource.Id).SingleOrDefaultAsync();
            return existsWithSameName == null;
        }
    }

    public virtual async Task<bool> CanInsertApiResourcePropertyAsync(ApiResourceProperty apiResourceProperty)
    {
        var existsWithSameName = await _dbContext.ApiResourceProperties.Where(x => x.Key == apiResourceProperty.Key
            && x.ApiResource.Id == apiResourceProperty.ApiResourceId).SingleOrDefaultAsync();
        return existsWithSameName == null;
    }

    /// <summary>
    ///     Add new api resource
    /// </summary>
    /// <param name="apiResource"></param>
    /// <returns>This method return new api resource id</returns>
    public virtual async Task<int> AddApiResourceAsync(ApiResource apiResource)
    {
        _dbContext.ApiResources.Add(apiResource);

        await AutoSaveChangesAsync();

        return apiResource.Id;
    }

    public virtual async Task<int> UpdateApiResourceAsync(ApiResource apiResource)
    {
        //Remove old relations
        await RemoveApiResourceClaimsAsync(apiResource);
        await RemoveApiResourceScopesAsync(apiResource);

        //Update with new data
        _dbContext.ApiResources.Update(apiResource);

        return await AutoSaveChangesAsync();
    }

    public virtual async Task<int> DeleteApiResourceAsync(ApiResource apiResource)
    {
        var resource = await _dbContext.ApiResources.Where(x => x.Id == apiResource.Id).SingleOrDefaultAsync();

        _dbContext.Remove(resource);

        return await AutoSaveChangesAsync();
    }


    public virtual async Task<PagedList<ApiResourceSecret>> GetApiSecretsAsync(int apiResourceId, int page = 1,
        int pageSize = 10)
    {
        var pagedList = new PagedList<ApiResourceSecret>();
        var apiSecrets = await _dbContext.ApiSecrets.Where(x => x.ApiResource.Id == apiResourceId)
            .PageBy(x => x.Id, page, pageSize).ToListAsync();

        pagedList.Data.AddRange(apiSecrets);
        pagedList.TotalCount = await _dbContext.ApiSecrets.Where(x => x.ApiResource.Id == apiResourceId).CountAsync();
        pagedList.PageSize = pageSize;

        return pagedList;
    }

    public virtual Task<ApiResourceSecret> GetApiSecretAsync(int apiSecretId)
    {
        return _dbContext.ApiSecrets
            .Include(x => x.ApiResource)
            .Where(x => x.Id == apiSecretId)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    public virtual async Task<int> AddApiSecretAsync(int apiResourceId, ApiResourceSecret apiSecret)
    {
        apiSecret.ApiResource = await _dbContext.ApiResources.Where(x => x.Id == apiResourceId).SingleOrDefaultAsync();
        await _dbContext.ApiSecrets.AddAsync(apiSecret);

        return await AutoSaveChangesAsync();
    }

    public virtual async Task<int> DeleteApiSecretAsync(ApiResourceSecret apiSecret)
    {
        var apiSecretToDelete = await _dbContext.ApiSecrets.Where(x => x.Id == apiSecret.Id).SingleOrDefaultAsync();
        _dbContext.ApiSecrets.Remove(apiSecretToDelete);

        return await AutoSaveChangesAsync();
    }

    public virtual async Task<int> SaveAllChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    public virtual async Task<string> GetApiResourceNameAsync(int apiResourceId)
    {
        var apiResourceName = await _dbContext.ApiResources.Where(x => x.Id == apiResourceId).Select(x => x.Name)
            .SingleOrDefaultAsync();

        return apiResourceName;
    }

    private async Task RemoveApiResourceClaimsAsync(ApiResource apiResource)
    {
        //Remove old api resource claims
        var apiResourceClaims =
            await _dbContext.ApiResourceClaims.Where(x => x.ApiResource.Id == apiResource.Id).ToListAsync();
        _dbContext.ApiResourceClaims.RemoveRange(apiResourceClaims);
    }

    private async Task RemoveApiResourceScopesAsync(ApiResource apiResource)
    {
        //Remove old api resource scopes
        var apiResourceScopes =
            await _dbContext.ApiResourceScopes.Where(x => x.ApiResource.Id == apiResource.Id).ToListAsync();
        _dbContext.ApiResourceScopes.RemoveRange(apiResourceScopes);
    }

    protected virtual async Task<int> AutoSaveChangesAsync()
    {
        return AutoSaveChanges ? await _dbContext.SaveChangesAsync() : (int)SavedStatus.WillBeSavedExplicitly;
    }
}