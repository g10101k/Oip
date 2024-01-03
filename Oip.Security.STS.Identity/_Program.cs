using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using Oip.Security.Common.Configuration.Helpers;
using Oip.Security.Shared.Configuration.Helpers;

namespace Oip.Security.STS.Identity;

public static class Program
{
    public static void Main(string[] args)
    {
        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        var configuration = GetConfiguration(args);

        try
        {
            DockerHelpers.ApplyDockerConfiguration(configuration);

            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            logger.Fatal(ex, "Host terminated unexpectedly");
            throw;
        }
        finally
        {
            LogManager.Shutdown();
        }
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
            .ConfigureAppConfiguration((hostContext, configApp) =>
            {
                var configurationRoot = configApp.Build();

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
            })
            .ConfigureLogging(x => x.ClearProviders())
            .UseNLog();
    }
}