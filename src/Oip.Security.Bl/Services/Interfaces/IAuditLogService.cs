using System;
using System.Threading.Tasks;
using Oip.Security.Bl.Dtos.Log;

namespace Oip.Security.Bl.Services.Interfaces;

public interface IAuditLogService
{
    Task<AuditLogsDto> GetAsync(AuditLogFilterDto filters);

    Task DeleteLogsOlderThanAsync(DateTime deleteOlderThan);
}