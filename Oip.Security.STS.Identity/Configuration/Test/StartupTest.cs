using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oip.Security.Dal.DbContexts;
using Oip.Security.STS.Identity.Helpers;

namespace Oip.Security.STS.Identity.Configuration.Test;

public class StartupTest : Startup
{
    public StartupTest(IWebHostEnvironment environment, IConfiguration configuration) : base(environment, configuration)
    {
    }

    public override void RegisterDbContexts(IServiceCollection services)
    {
        services
            .RegisterDbContextsStaging<AdminIdentityDbContext, IdentityServerConfigurationDbContext,
                IdentityServerPersistedGrantDbContext, IdentityServerDataProtectionDbContext>();
    }
}