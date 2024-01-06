using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Oip.Security.Dal.Interfaces;

namespace Oip.Security.Dal.DbContexts;

public class IdentityServerPersistedGrantDbContext : PersistedGrantDbContext<IdentityServerPersistedGrantDbContext>,
    IAdminPersistedGrantDbContext
{
    public IdentityServerPersistedGrantDbContext(DbContextOptions<IdentityServerPersistedGrantDbContext> options,
        OperationalStoreOptions storeOptions)
        : base(options, storeOptions)
    {
    }
}