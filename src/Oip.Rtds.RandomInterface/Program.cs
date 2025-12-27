using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using Oip.Rtds.Base;
using Oip.Rtds.Base.Services;
using Oip.Rtds.Grpc;
using Oip.Rtds.RandomInterface.Services;
using Oip.Rtds.RandomInterface.Settings;

namespace Oip.Rtds.RandomInterface;

public class Program
{
    public static async Task Main(string[] args)
    {
        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        try
        {
            var settings = AppSettings.Initialize(args, false, false);
            var builder = Host.CreateApplicationBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddNLog();
            builder.Services.AddGrpcClient<RtdsService.RtdsServiceClient>(options =>
            {
                options.Address = new Uri(settings.RtdsUrl);
            });
            builder.Services.AddScoped<UpdateTagInfoService>();
            builder.Services.AddScoped<TagWorkerService>();
            builder.Services.AddSingleton<FormulaManager>();
            builder.Services.AddSingleton<TagCacheService>();
            builder.Services.AddSingleton<BufferWriterService>();
            builder.Services.AddSingleton<CompressService>();
            builder.Services.AddHostedService<Worker>();
            builder.Services.AddHostedService<UpdateTagInfoHostedService>();
            builder.Services.AddHostedService<TagWorkerHostedService>();
            var host = builder.Build();
            await host.RunAsync();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}