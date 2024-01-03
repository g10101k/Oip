using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Oip.Security.Bl.Dtos.Configuration;
using Oip.Security.Bl.Events.ApiResource;
using Oip.Security.Bl.Mappers;
using Oip.Security.Bl.Resources;
using Oip.Security.Bl.Services.Interfaces;
using Oip.Security.Bl.Shared.ExceptionHandling;
using Oip.Security.Dal.Helpers;
using Oip.Security.Dal.Repositories.Interfaces;
using Skoruba.AuditLogging.Services;

namespace Oip.Security.Bl.Services;

public class ApiResourceService : IApiResourceService
{
    private const string SharedSecret = "SharedSecret";
    private readonly IApiResourceRepository _apiResourceRepository;
    private readonly IApiResourceServiceResources _apiResourceServiceResources;
    private readonly IAuditEventLogger _auditEventLogger;
    private readonly IClientService _clientService;

    public ApiResourceService(IApiResourceRepository apiResourceRepository,
        IApiResourceServiceResources apiResourceServiceResources,
        IClientService clientService,
        IAuditEventLogger auditEventLogger)
    {
        _apiResourceRepository = apiResourceRepository;
        _apiResourceServiceResources = apiResourceServiceResources;
        _clientService = clientService;
        _auditEventLogger = auditEventLogger;
    }

    public virtual async Task<ApiResourcesDto> GetApiResourcesAsync(string search, int page = 1, int pageSize = 10)
    {
        var pagedList = await _apiResourceRepository.GetApiResourcesAsync(search, page, pageSize);
        var apiResourcesDto = pagedList.ToModel();

        await _auditEventLogger.LogEventAsync(new ApiResourcesRequestedEvent(apiResourcesDto));

        return apiResourcesDto;
    }

    public virtual async Task<ApiResourcePropertiesDto> GetApiResourcePropertiesAsync(int apiResourceId, int page = 1,
        int pageSize = 10)
    {
        var apiResource = await _apiResourceRepository.GetApiResourceAsync(apiResourceId);
        if (apiResource == null)
            throw new UserFriendlyErrorPageException(
                string.Format(_apiResourceServiceResources.ApiResourceDoesNotExist().Description, apiResourceId),
                _apiResourceServiceResources.ApiResourceDoesNotExist().Description);

        var pagedList = await _apiResourceRepository.GetApiResourcePropertiesAsync(apiResourceId, page, pageSize);
        var apiResourcePropertiesDto = pagedList.ToModel();
        apiResourcePropertiesDto.ApiResourceId = apiResourceId;
        apiResourcePropertiesDto.ApiResourceName = await _apiResourceRepository.GetApiResourceNameAsync(apiResourceId);

        await _auditEventLogger.LogEventAsync(
            new ApiResourcePropertiesRequestedEvent(apiResourceId, apiResourcePropertiesDto));

        return apiResourcePropertiesDto;
    }

    public virtual async Task<ApiResourcePropertiesDto> GetApiResourcePropertyAsync(int apiResourcePropertyId)
    {
        var apiResourceProperty = await _apiResourceRepository.GetApiResourcePropertyAsync(apiResourcePropertyId);
        if (apiResourceProperty == null)
            throw new UserFriendlyErrorPageException(string.Format(
                _apiResourceServiceResources.ApiResourcePropertyDoesNotExist().Description, apiResourcePropertyId));

        var apiResourcePropertiesDto = apiResourceProperty.ToModel();
        apiResourcePropertiesDto.ApiResourceId = apiResourceProperty.ApiResourceId;
        apiResourcePropertiesDto.ApiResourceName =
            await _apiResourceRepository.GetApiResourceNameAsync(apiResourceProperty.ApiResourceId);

        await _auditEventLogger.LogEventAsync(
            new ApiResourcePropertyRequestedEvent(apiResourcePropertyId, apiResourcePropertiesDto));

        return apiResourcePropertiesDto;
    }

    public virtual async Task<int> AddApiResourcePropertyAsync(ApiResourcePropertiesDto apiResourceProperties)
    {
        var canInsert = await CanInsertApiResourcePropertyAsync(apiResourceProperties);
        if (!canInsert)
        {
            await BuildApiResourcePropertiesViewModelAsync(apiResourceProperties);
            throw new UserFriendlyViewException(
                string.Format(_apiResourceServiceResources.ApiResourcePropertyExistsValue().Description,
                    apiResourceProperties.Key), _apiResourceServiceResources.ApiResourcePropertyExistsKey().Description,
                apiResourceProperties);
        }

        var apiResourceProperty = apiResourceProperties.ToEntity();

        var saved = await _apiResourceRepository.AddApiResourcePropertyAsync(apiResourceProperties.ApiResourceId,
            apiResourceProperty);

        await _auditEventLogger.LogEventAsync(new ApiResourcePropertyAddedEvent(apiResourceProperties));

        return saved;
    }

    public virtual async Task<int> DeleteApiResourcePropertyAsync(ApiResourcePropertiesDto apiResourceProperty)
    {
        var propertyEntity = apiResourceProperty.ToEntity();

        var deleted = await _apiResourceRepository.DeleteApiResourcePropertyAsync(propertyEntity);

        await _auditEventLogger.LogEventAsync(new ApiResourcePropertyDeletedEvent(apiResourceProperty));

        return deleted;
    }

    public virtual async Task<bool> CanInsertApiResourcePropertyAsync(ApiResourcePropertiesDto apiResourceProperty)
    {
        var resource = apiResourceProperty.ToEntity();

        return await _apiResourceRepository.CanInsertApiResourcePropertyAsync(resource);
    }

    public virtual ApiSecretsDto BuildApiSecretsViewModel(ApiSecretsDto apiSecrets)
    {
        apiSecrets.HashTypes = _clientService.GetHashTypes();
        apiSecrets.TypeList = _clientService.GetSecretTypes();

        return apiSecrets;
    }

    public virtual async Task<ApiResourceDto> GetApiResourceAsync(int apiResourceId)
    {
        var apiResource = await _apiResourceRepository.GetApiResourceAsync(apiResourceId);
        if (apiResource == null)
            throw new UserFriendlyErrorPageException(_apiResourceServiceResources.ApiResourceDoesNotExist().Description,
                _apiResourceServiceResources.ApiResourceDoesNotExist().Description);
        var apiResourceDto = apiResource.ToModel();

        await _auditEventLogger.LogEventAsync(new ApiResourceRequestedEvent(apiResourceId, apiResourceDto));

        return apiResourceDto;
    }

    public virtual async Task<int> AddApiResourceAsync(ApiResourceDto apiResource)
    {
        var canInsert = await CanInsertApiResourceAsync(apiResource);
        if (!canInsert)
            throw new UserFriendlyViewException(
                string.Format(_apiResourceServiceResources.ApiResourceExistsValue().Description, apiResource.Name),
                _apiResourceServiceResources.ApiResourceExistsKey().Description, apiResource);

        var resource = apiResource.ToEntity();

        var added = await _apiResourceRepository.AddApiResourceAsync(resource);

        await _auditEventLogger.LogEventAsync(new ApiResourceAddedEvent(apiResource));

        return added;
    }

    public virtual async Task<int> UpdateApiResourceAsync(ApiResourceDto apiResource)
    {
        var canInsert = await CanInsertApiResourceAsync(apiResource);
        if (!canInsert)
            throw new UserFriendlyViewException(
                string.Format(_apiResourceServiceResources.ApiResourceExistsValue().Description, apiResource.Name),
                _apiResourceServiceResources.ApiResourceExistsKey().Description, apiResource);

        var resource = apiResource.ToEntity();

        var originalApiResource = await GetApiResourceAsync(apiResource.Id);

        var updated = await _apiResourceRepository.UpdateApiResourceAsync(resource);

        await _auditEventLogger.LogEventAsync(new ApiResourceUpdatedEvent(originalApiResource, apiResource));

        return updated;
    }

    public virtual async Task<int> DeleteApiResourceAsync(ApiResourceDto apiResource)
    {
        var resource = apiResource.ToEntity();

        var deleted = await _apiResourceRepository.DeleteApiResourceAsync(resource);

        await _auditEventLogger.LogEventAsync(new ApiResourceDeletedEvent(apiResource));

        return deleted;
    }

    public virtual async Task<bool> CanInsertApiResourceAsync(ApiResourceDto apiResource)
    {
        var resource = apiResource.ToEntity();

        return await _apiResourceRepository.CanInsertApiResourceAsync(resource);
    }

    public virtual async Task<ApiSecretsDto> GetApiSecretsAsync(int apiResourceId, int page = 1, int pageSize = 10)
    {
        var apiResource = await _apiResourceRepository.GetApiResourceAsync(apiResourceId);
        if (apiResource == null)
            throw new UserFriendlyErrorPageException(
                string.Format(_apiResourceServiceResources.ApiResourceDoesNotExist().Description, apiResourceId),
                _apiResourceServiceResources.ApiResourceDoesNotExist().Description);

        var pagedList = await _apiResourceRepository.GetApiSecretsAsync(apiResourceId, page, pageSize);

        var apiSecretsDto = pagedList.ToModel();
        apiSecretsDto.ApiResourceId = apiResourceId;
        apiSecretsDto.ApiResourceName = await _apiResourceRepository.GetApiResourceNameAsync(apiResourceId);

        // remove secret value from dto
        apiSecretsDto.ApiSecrets.ForEach(x => x.Value = null);

        await _auditEventLogger.LogEventAsync(new ApiSecretsRequestedEvent(apiSecretsDto.ApiResourceId,
            apiSecretsDto.ApiSecrets.Select(x => (x.Id, x.Type, x.Expiration)).ToList()));

        return apiSecretsDto;
    }

    public virtual async Task<int> AddApiSecretAsync(ApiSecretsDto apiSecret)
    {
        HashApiSharedSecret(apiSecret);

        var secret = apiSecret.ToEntity();

        var added = await _apiResourceRepository.AddApiSecretAsync(apiSecret.ApiResourceId, secret);

        await _auditEventLogger.LogEventAsync(new ApiSecretAddedEvent(apiSecret.ApiResourceId, apiSecret.Type,
            apiSecret.Expiration));

        return added;
    }

    public virtual async Task<ApiSecretsDto> GetApiSecretAsync(int apiSecretId)
    {
        var apiSecret = await _apiResourceRepository.GetApiSecretAsync(apiSecretId);
        if (apiSecret == null)
            throw new UserFriendlyErrorPageException(
                string.Format(_apiResourceServiceResources.ApiSecretDoesNotExist().Description, apiSecretId),
                _apiResourceServiceResources.ApiSecretDoesNotExist().Description);
        var apiSecretsDto = apiSecret.ToModel();

        // remove secret value for dto
        apiSecretsDto.Value = null;

        await _auditEventLogger.LogEventAsync(new ApiSecretRequestedEvent(apiSecretsDto.ApiResourceId,
            apiSecretsDto.ApiSecretId, apiSecretsDto.Type, apiSecretsDto.Expiration));

        return apiSecretsDto;
    }

    public virtual async Task<int> DeleteApiSecretAsync(ApiSecretsDto apiSecret)
    {
        var secret = apiSecret.ToEntity();

        var deleted = await _apiResourceRepository.DeleteApiSecretAsync(secret);

        await _auditEventLogger.LogEventAsync(new ApiSecretDeletedEvent(apiSecret.ApiResourceId, apiSecret.ApiSecretId));

        return deleted;
    }

    public virtual async Task<string> GetApiResourceNameAsync(int apiResourceId)
    {
        return await _apiResourceRepository.GetApiResourceNameAsync(apiResourceId);
    }

    private void HashApiSharedSecret(ApiSecretsDto apiSecret)
    {
        if (apiSecret.Type != SharedSecret) return;

        if (apiSecret.HashTypeEnum == HashType.Sha256)
            apiSecret.Value = apiSecret.Value.Sha256();
        else if (apiSecret.HashTypeEnum == HashType.Sha512) apiSecret.Value = apiSecret.Value.Sha512();
    }

    private async Task BuildApiResourcePropertiesViewModelAsync(ApiResourcePropertiesDto apiResourceProperties)
    {
        var apiResourcePropertiesDto = await GetApiResourcePropertiesAsync(apiResourceProperties.ApiResourceId);
        apiResourceProperties.ApiResourceProperties.AddRange(apiResourcePropertiesDto.ApiResourceProperties);
        apiResourceProperties.TotalCount = apiResourcePropertiesDto.TotalCount;
    }
}