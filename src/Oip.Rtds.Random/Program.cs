using NLog;
using NLog.Web;
using Oip.Rtds.Random.Settings;
using NLog.Extensions.Logging;

namespace Oip.Rtds.Random;

public class Program
{
    public static async Task Main(string[] args)
    {
        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        try
        {
            AppSettings.Initialize(args, false, false);
            var builder = Host.CreateApplicationBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddNLog();
            builder.Services.AddHostedService<Worker>();
            var host = builder.Build();
            await host.RunAsync();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}