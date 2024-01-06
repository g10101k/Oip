using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Oip.Security.Configuration.Test;

public class StartupTest : Startup
{
    public StartupTest(IWebHostEnvironment env, IConfiguration configuration) : base(env, configuration)
    {
    }

    public override void ConfigureUiOptions(IdentityServer4AdminUiOptions options)
    {
        base.ConfigureUiOptions(options);

        // Use staging DbContexts and auth services.
        options.Testing.IsStaging = true;
    }
}