using Microsoft.EntityFrameworkCore;
using Oip.Security.Dal.Constants;
using Oip.Security.Dal.Entities;
using Oip.Security.Dal.Interfaces;

namespace Oip.Security.Dal.DbContexts;

public class AdminLogDbContext : DbContext, IAdminLogDbContext
{
    public AdminLogDbContext(DbContextOptions<AdminLogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Log> Logs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Log>(log =>
        {
            log.ToTable(TableConsts.Logging);
            log.HasKey(x => x.Id);
            log.Property(x => x.Level).HasMaxLength(128);
        });
    }
}