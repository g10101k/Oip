﻿using System.Threading.Tasks;
using Oip.Security.Bl.Dtos.Grant;
using Oip.Security.Bl.Events.PersistedGrant;
using Oip.Security.Bl.Mappers;
using Oip.Security.Bl.Resources;
using Oip.Security.Bl.Services.Interfaces;
using Oip.Security.Bl.Shared.ExceptionHandling;
using Oip.Security.Dal.Identity.Repositories.Interfaces;
using Skoruba.AuditLogging.Services;

namespace Oip.Security.Bl.Services;

public class PersistedGrantAspNetIdentityService : IPersistedGrantAspNetIdentityService
{
    protected readonly IAuditEventLogger AuditEventLogger;
    protected readonly IPersistedGrantAspNetIdentityRepository PersistedGrantAspNetIdentityRepository;
    protected readonly IPersistedGrantAspNetIdentityServiceResources PersistedGrantAspNetIdentityServiceResources;

    public PersistedGrantAspNetIdentityService(
        IPersistedGrantAspNetIdentityRepository persistedGrantAspNetIdentityRepository,
        IPersistedGrantAspNetIdentityServiceResources persistedGrantAspNetIdentityServiceResources,
        IAuditEventLogger auditEventLogger)
    {
        PersistedGrantAspNetIdentityRepository = persistedGrantAspNetIdentityRepository;
        PersistedGrantAspNetIdentityServiceResources = persistedGrantAspNetIdentityServiceResources;
        AuditEventLogger = auditEventLogger;
    }

    public virtual async Task<PersistedGrantsDto> GetPersistedGrantsByUsersAsync(string search, int page = 1,
        int pageSize = 10)
    {
        var pagedList =
            await PersistedGrantAspNetIdentityRepository.GetPersistedGrantsByUsersAsync(search, page, pageSize);
        var persistedGrantsDto = pagedList.ToModel();

        await AuditEventLogger.LogEventAsync(new PersistedGrantsIdentityByUsersRequestedEvent(persistedGrantsDto));

        return persistedGrantsDto;
    }

    public virtual async Task<PersistedGrantsDto> GetPersistedGrantsByUserAsync(string subjectId, int page = 1,
        int pageSize = 10)
    {
        var exists = await PersistedGrantAspNetIdentityRepository.ExistsPersistedGrantsAsync(subjectId);
        if (!exists)
            throw new UserFriendlyErrorPageException(
                string.Format(
                    PersistedGrantAspNetIdentityServiceResources.PersistedGrantWithSubjectIdDoesNotExist().Description,
                    subjectId),
                PersistedGrantAspNetIdentityServiceResources.PersistedGrantWithSubjectIdDoesNotExist().Description);

        var pagedList =
            await PersistedGrantAspNetIdentityRepository.GetPersistedGrantsByUserAsync(subjectId, page, pageSize);
        var persistedGrantsDto = pagedList.ToModel();

        await AuditEventLogger.LogEventAsync(new PersistedGrantsIdentityByUserRequestedEvent(persistedGrantsDto));

        return persistedGrantsDto;
    }

    public virtual async Task<PersistedGrantDto> GetPersistedGrantAsync(string key)
    {
        var persistedGrant = await PersistedGrantAspNetIdentityRepository.GetPersistedGrantAsync(key);
        if (persistedGrant == null)
            throw new UserFriendlyErrorPageException(
                string.Format(PersistedGrantAspNetIdentityServiceResources.PersistedGrantDoesNotExist().Description,
                    key), PersistedGrantAspNetIdentityServiceResources.PersistedGrantDoesNotExist().Description);
        var persistedGrantDto = persistedGrant.ToModel();

        await AuditEventLogger.LogEventAsync(new PersistedGrantIdentityRequestedEvent(persistedGrantDto));

        return persistedGrantDto;
    }

    public virtual async Task<int> DeletePersistedGrantAsync(string key)
    {
        var exists = await PersistedGrantAspNetIdentityRepository.ExistsPersistedGrantAsync(key);
        if (!exists)
            throw new UserFriendlyErrorPageException(
                string.Format(PersistedGrantAspNetIdentityServiceResources.PersistedGrantDoesNotExist().Description,
                    key), PersistedGrantAspNetIdentityServiceResources.PersistedGrantDoesNotExist().Description);

        var deleted = await PersistedGrantAspNetIdentityRepository.DeletePersistedGrantAsync(key);

        await AuditEventLogger.LogEventAsync(new PersistedGrantIdentityDeletedEvent(key));

        return deleted;
    }

    public virtual async Task<int> DeletePersistedGrantsAsync(string userId)
    {
        var exists = await PersistedGrantAspNetIdentityRepository.ExistsPersistedGrantsAsync(userId);
        if (!exists)
            throw new UserFriendlyErrorPageException(
                string.Format(
                    PersistedGrantAspNetIdentityServiceResources.PersistedGrantWithSubjectIdDoesNotExist().Description,
                    userId),
                PersistedGrantAspNetIdentityServiceResources.PersistedGrantWithSubjectIdDoesNotExist().Description);

        var deleted = await PersistedGrantAspNetIdentityRepository.DeletePersistedGrantsAsync(userId);

        await AuditEventLogger.LogEventAsync(new PersistedGrantsIdentityDeletedEvent(userId));

        return deleted;
    }
}