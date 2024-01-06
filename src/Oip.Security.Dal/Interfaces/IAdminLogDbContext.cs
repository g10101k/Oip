using Microsoft.EntityFrameworkCore;
using Oip.Security.Dal.Entities;

namespace Oip.Security.Dal.Interfaces;

public interface IAdminLogDbContext
{
    DbSet<Log> Logs { get; set; }
}