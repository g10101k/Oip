using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using Oip.Security.Common.Configuration.Helpers;
using Oip.Security.Dal.Common.Entities.Identity;
using Oip.Security.Dal.Configuration;
using Oip.Security.Dal.DbContexts;
using Oip.Security.Dal.Shared.Entities.Identity;
using Oip.Security.Dal.Shared.Helpers;
using Oip.Security.Shared.Configuration.Helpers;

namespace Oip.Security;

internal static class Program
{
    private const string SeedArgs = "/seed";
    private const string MigrateOnlyArgs = "/migrateonly";

    public static async Task Main(string[] args)
    {
        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        try
        {
            var configuration = GetConfiguration(args);

            DockerHelpers.ApplyDockerConfiguration(configuration);

            var host = CreateHostBuilder(args).Build();

            var migrationComplete = await ApplyDbMigrationsWithDataSeedAsync(args, configuration, host);
            if (args.Any(x => x == MigrateOnlyArgs))
            {
                await host.StopAsync();
                if (!migrationComplete) Environment.ExitCode = -1;

                return;
            }

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            logger.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            LogManager.Shutdown();
        }
    }

    private static async Task<bool> ApplyDbMigrationsWithDataSeedAsync(string[] args, IConfiguration configuration,
        IHost host)
    {
        var applyDbMigrationWithDataSeedFromProgramArguments = args.Any(x => x == SeedArgs);
        if (applyDbMigrationWithDataSeedFromProgramArguments) args = args.Except(new[] { SeedArgs }).ToArray();

        var seedConfiguration = configuration.GetSection(nameof(SeedConfiguration)).Get<SeedConfiguration>();
        var databaseMigrationsConfiguration = configuration.GetSection(nameof(DatabaseMigrationsConfiguration))
            .Get<DatabaseMigrationsConfiguration>();

        return await DbMigrationHelpers
            .ApplyDbMigrationsWithDataSeedAsync<IdentityServerConfigurationDbContext, AdminIdentityDbContext,
                IdentityServerPersistedGrantDbContext, AdminLogDbContext, AdminAuditLogDbContext,
                IdentityServerDataProtectionDbContext, UserIdentity, UserIdentityRole>(host,
                applyDbMigrationWithDataSeedFromProgramArguments, seedConfiguration, databaseMigrationsConfiguration);
    }

    private static IConfiguration GetConfiguration(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var isDevelopment = environment == Environments.Development;

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{environment}.json", true, true);

        if (isDevelopment) configurationBuilder.AddUserSecrets<Startup>(true);

        var configuration = configurationBuilder.Build();

        configuration.AddAzureKeyVaultConfiguration(configurationBuilder);

        configurationBuilder.AddCommandLine(args);
        configurationBuilder.AddEnvironmentVariables();

        return configurationBuilder.Build();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureLogging(x =>
                x.ClearProviders()
            )
            .ConfigureAppConfiguration((hostContext, configApp) =>
            {
                var configurationRoot = configApp.Build();

                configApp.AddJsonFile("identitydata.json", true, true);
                configApp.AddJsonFile("identityserverdata.json", true, true);

                var env = hostContext.HostingEnvironment;

                if (env.IsDevelopment()) configApp.AddUserSecrets<Startup>(true);

                configurationRoot.AddAzureKeyVaultConfiguration(configApp);

                configApp.AddEnvironmentVariables();
                configApp.AddCommandLine(args);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(options => options.AddServerHeader = false);
                webBuilder.UseStartup<Startup>();
            });
    }
}