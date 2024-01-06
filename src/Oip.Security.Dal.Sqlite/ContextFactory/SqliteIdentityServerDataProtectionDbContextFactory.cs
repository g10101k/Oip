using Oip.Security.Dal.DbContexts;

namespace Oip.Security.Dal.Sqlite.ContextFactory;

// ReSharper disable once UnusedType.Global
public class SqliteIdentityServerDataProtectionDbContextFactory :
    SqliteBaseDbContextFactory<IdentityServerDataProtectionDbContext>
{
}