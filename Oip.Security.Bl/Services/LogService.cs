using System;
using System.Threading.Tasks;
using Oip.Security.Bl.Dtos.Log;
using Oip.Security.Bl.Events.Log;
using Oip.Security.Bl.Mappers;
using Oip.Security.Bl.Services.Interfaces;
using Oip.Security.Dal.Repositories.Interfaces;
using Skoruba.AuditLogging.Services;

namespace Oip.Security.Bl.Services;

public class LogService : ILogService
{
    protected readonly IAuditEventLogger AuditEventLogger;
    protected readonly ILogRepository Repository;

    public LogService(ILogRepository repository, IAuditEventLogger auditEventLogger)
    {
        Repository = repository;
        AuditEventLogger = auditEventLogger;
    }

    public virtual async Task<LogsDto> GetLogsAsync(string search, int page = 1, int pageSize = 10)
    {
        var pagedList = await Repository.GetLogsAsync(search, page, pageSize);
        var logs = pagedList.ToModel();

        await AuditEventLogger.LogEventAsync(new LogsRequestedEvent());

        return logs;
    }

    public virtual async Task DeleteLogsOlderThanAsync(DateTime deleteOlderThan)
    {
        await Repository.DeleteLogsOlderThanAsync(deleteOlderThan);

        await AuditEventLogger.LogEventAsync(new LogsDeletedEvent(deleteOlderThan));
    }
}