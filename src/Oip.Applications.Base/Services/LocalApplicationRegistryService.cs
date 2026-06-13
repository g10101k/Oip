using Microsoft.AspNetCore.Http;
using Oip.Applications.Base.Contracts;
using Oip.Applications.Base.Data;
using Oip.Base.Exceptions;
using Oip.Base.Settings;

namespace Oip.Applications.Base.Services;

/// <summary>
/// Local application registry implementation backed by EF storage.
/// </summary>
public class LocalApplicationRegistryService(
    ApplicationRegistryRepository repository,
    IBaseOipModuleAppSettings appSettings)
    : IApplicationRegistryService
{
    public async Task<ApplicationRegistryItemDto> RegisterApplicationAsync(
        ApplicationRegistryItemDto application,
        CancellationToken cancellationToken = default)
    {
        Validate(application);

        var entity = await repository.GetByCodeAsync(application.Code, cancellationToken);
        if (entity is null)
        {
            entity = MapToEntity(application);
            await repository.AddAsync(entity, cancellationToken);
            return MapToDto(entity);
        }

        Apply(entity, application);
        await repository.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task<IReadOnlyList<ApplicationRegistryItemDto>> GetApplicationRegistryItemsAsync(
        CancellationToken cancellationToken = default)
    {
        var applications = await repository.GetAllAsync(cancellationToken);
        return applications
            .Where(x => x.Enabled)
            .OrderBy(x => x.Order)
            .ThenBy(x => x.DisplayName)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<ApplicationRegistryItemDto> GetApplicationRegistryItemByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByCodeAsync(code, cancellationToken);
        if (entity is null)
            throw NotFound(code);

        return MapToDto(entity);
    }

    public async Task<ApplicationRegistryItemDto> CreateApplicationRegistryItemAsync(
        ApplicationRegistryItemDto application,
        CancellationToken cancellationToken = default)
    {
        Validate(application);

        if (await repository.GetByCodeAsync(application.Code, cancellationToken) is not null)
            throw new ApiException(
                "Application registry item already exists",
                $"Application registry item '{application.Code}' already exists.",
                StatusCodes.Status400BadRequest);

        var entity = await repository.AddAsync(MapToEntity(application), cancellationToken);
        return MapToDto(entity);
    }

    public async Task<ApplicationRegistryItemDto> UpdateApplicationRegistryItemAsync(
        string code,
        ApplicationRegistryItemDto application,
        CancellationToken cancellationToken = default)
    {
        Validate(application);

        if (!string.Equals(code.Trim(), application.Code.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new ApiException(
                "Application code cannot be changed",
                "Changing application code is treated as delete/create.",
                StatusCodes.Status400BadRequest);

        var entity = await repository.GetByCodeAsync(code, cancellationToken);
        if (entity is null)
            throw NotFound(code);

        Apply(entity, application);
        await repository.SaveChangesAsync(cancellationToken);
        return MapToDto(entity);
    }

    public async Task DeleteApplicationRegistryItemAsync(string code, CancellationToken cancellationToken = default)
    {
        if (await repository.GetByCodeAsync(code, cancellationToken) is null)
            throw NotFound(code);

        await repository.DeleteByCodeAsync(code, cancellationToken);
    }

    private ApplicationRegistryItemDto MapToDto(ApplicationRegistryItemEntity entity)
    {
        return new ApplicationRegistryItemDto
        {
            Code = entity.Code,
            DisplayName = entity.DisplayName,
            BaseUrl = NormalizeUrl(entity.BaseUrl),
            ApiBaseUrl = NormalizeUrl(entity.ApiBaseUrl),
            Icon = entity.Icon,
            Order = entity.Order,
            Enabled = entity.Enabled,
            ServiceType = entity.ServiceType,
            IsCurrent = string.Equals(entity.Code, appSettings.Application.Code, StringComparison.OrdinalIgnoreCase)
        };
    }

    private static ApplicationRegistryItemEntity MapToEntity(ApplicationRegistryItemDto application)
    {
        var entity = new ApplicationRegistryItemEntity();
        Apply(entity, application);
        return entity;
    }

    private static void Apply(ApplicationRegistryItemEntity entity, ApplicationRegistryItemDto application)
    {
        entity.Code = application.Code.Trim();
        entity.DisplayName = application.DisplayName.Trim();
        entity.BaseUrl = NormalizeUrl(application.BaseUrl);
        entity.ApiBaseUrl = NormalizeUrl(application.ApiBaseUrl);
        entity.Icon = string.IsNullOrWhiteSpace(application.Icon) ? "pi pi-circle" : application.Icon.Trim();
        entity.Order = application.Order;
        entity.Enabled = application.Enabled;
        entity.ServiceType = application.ServiceType;
    }

    private static void Validate(ApplicationRegistryItemDto application)
    {
        if (string.IsNullOrWhiteSpace(application.Code))
            throw new ApiException(
                "Application code is required",
                "Application registry item code must be specified.",
                StatusCodes.Status400BadRequest);

        if (string.IsNullOrWhiteSpace(application.DisplayName))
            throw new ApiException(
                "Application display name is required",
                "Application registry item display name must be specified.",
                StatusCodes.Status400BadRequest);

        if (string.IsNullOrWhiteSpace(application.BaseUrl))
            throw new ApiException(
                "Application base URL is required",
                "Application registry item base URL must be specified.",
                StatusCodes.Status400BadRequest);

        if (string.IsNullOrWhiteSpace(application.ApiBaseUrl))
            throw new ApiException(
                "Application API base URL is required",
                "Application registry item API base URL must be specified.",
                StatusCodes.Status400BadRequest);
    }

    private static string NormalizeUrl(string url)
    {
        return url.Trim().TrimEnd('/');
    }

    private static ApiException NotFound(string code)
    {
        return new ApiException(
            "Application registry item not found",
            $"Application registry item '{code}' was not found.",
            StatusCodes.Status404NotFound);
    }
}
