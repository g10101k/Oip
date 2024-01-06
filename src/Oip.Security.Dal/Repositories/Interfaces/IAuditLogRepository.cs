using System;
using System.Threading.Tasks;
using Oip.Security.Dal.Common;
using Skoruba.AuditLogging.EntityFramework.Entities;

namespace Oip.Security.Dal.Repositories.Interfaces;

public interface IAuditLogRepository<TAuditLog> where TAuditLog : AuditLog
{
    bool AutoSaveChanges { get; set; }

    Task<PagedList<TAuditLog>> GetAsync(string @event, string source, string category, DateTime? created,
        string subjectIdentifier, string subjectName, int page = 1, int pageSize = 10);

    Task DeleteLogsOlderThanAsync(DateTime deleteOlderThan);
}