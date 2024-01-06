using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Oip.Security.Dal.Common.Entities.Identity;
using Oip.Security.Dal.Constants;
using Oip.Security.Dal.Shared.Entities.Identity;

namespace Oip.Security.Dal.DbContexts;

public class AdminIdentityDbContext :
    IdentityDbContext<UserIdentity, UserIdentityRole, string, UserIdentityUserClaim,
        UserIdentityUserRole, UserIdentityUserLogin, UserIdentityRoleClaim, UserIdentityUserToken>
{
    public AdminIdentityDbContext(DbContextOptions<AdminIdentityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<UserIdentityRole>().ToTable(TableConsts.IdentityRoles);
        builder.Entity<UserIdentityRoleClaim>().ToTable(TableConsts.IdentityRoleClaims);
        builder.Entity<UserIdentityUserRole>().ToTable(TableConsts.IdentityUserRoles);
        builder.Entity<UserIdentity>().ToTable(TableConsts.IdentityUsers);
        builder.Entity<UserIdentityUserLogin>().ToTable(TableConsts.IdentityUserLogins);
        builder.Entity<UserIdentityUserClaim>().ToTable(TableConsts.IdentityUserClaims);
        builder.Entity<UserIdentityUserToken>().ToTable(TableConsts.IdentityUserTokens);
    }
}