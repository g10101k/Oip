using System;
using System.Threading.Tasks;
using Oip.Security.Dal.Common;
using Oip.Security.Dal.Entities;

namespace Oip.Security.Dal.Repositories.Interfaces;

public interface ILogRepository
{
    bool AutoSaveChanges { get; set; }
    Task<PagedList<Log>> GetLogsAsync(string search, int page = 1, int pageSize = 10);

    Task DeleteLogsOlderThanAsync(DateTime deleteOlderThan);
}