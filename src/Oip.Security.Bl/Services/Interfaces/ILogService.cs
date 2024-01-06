using System;
using System.Threading.Tasks;
using Oip.Security.Bl.Dtos.Log;

namespace Oip.Security.Bl.Services.Interfaces;

public interface ILogService
{
    Task<LogsDto> GetLogsAsync(string search, int page = 1, int pageSize = 10);

    Task DeleteLogsOlderThanAsync(DateTime deleteOlderThan);
}