using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oip.Security.Common.Configuration.Helpers;
using Oip.Security.Configuration.Database;
using Oip.Security.Dal.Common.Entities.Identity;
using Oip.Security.Dal.DbContexts;
using Oip.Security.Dal.Shared.Entities.Identity;
using Oip.Security.Helpers;
using Oip.Security.Shared.Dtos;
using Oip.Security.Shared.Dtos.Identity;
using Skoruba.AuditLogging.EntityFramework.Entities;

namespace Oip.Security;

public class Startup
{
    public Startup(IWebHostEnvironment env, IConfiguration configuration)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        HostingEnvironment = env;
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; }

    private IWebHostEnvironment HostingEnvironment { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Adds the IdentityServer4 Admin UI with custom options.
        services
            .AddIdentityServer4AdminUi<AdminIdentityDbContext, IdentityServerConfigurationDbContext,
                IdentityServerPersistedGrantDbContext,
                AdminLogDbContext, AdminAuditLogDbContext, AuditLog, IdentityServerDataProtectionDbContext,
                UserIdentity, UserIdentityRole, UserIdentityUserClaim, UserIdentityUserRole,
                UserIdentityUserLogin, UserIdentityRoleClaim, UserIdentityUserToken, string,
                IdentityUserDto, IdentityRoleDto, IdentityUsersDto, IdentityRolesDto, IdentityUserRolesDto,
                IdentityUserClaimsDto, IdentityUserProviderDto, IdentityUserProvidersDto, IdentityUserChangePasswordDto,
                IdentityRoleClaimsDto, IdentityUserClaimDto, IdentityRoleClaimDto>(ConfigureUiOptions);

        // Monitor changes in Admin UI views
        services.AddAdminUiRazorRuntimeCompilation(HostingEnvironment);

        // Add email senders which is currently setup for SendGrid and SMTP
        services.AddEmailSenders(Configuration);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        app.UseRouting();

        app.UseIdentityServer4AdminUI();

        app.UseEndpoints(endpoint =>
        {
            endpoint.MapIdentityServer4AdminUI();
            endpoint.MapIdentityServer4AdminUIHealthChecks();
        });
    }

    public virtual void ConfigureUiOptions(IdentityServer4AdminUiOptions options)
    {
        // Applies configuration from appsettings.
        options.BindConfiguration(Configuration);
        if (HostingEnvironment.IsDevelopment())
            options.Security.UseDeveloperExceptionPage = true;
        else
            options.Security.UseHsts = true;

        // Set migration assembly for application of db migrations
        var migrationsAssembly =
            MigrationAssemblyConfiguration.GetMigrationAssemblyByProvider(options.DatabaseProvider);
        options.DatabaseMigrations.SetMigrationsAssemblies(migrationsAssembly);

        // Use production DbContexts and auth services.
        options.Testing.IsStaging = false;
    }
}