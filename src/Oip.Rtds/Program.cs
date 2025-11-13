using NLog;
using NLog.Web;
using Oip.Base.Extensions;
using Oip.Rtds.Data;
using Oip.Rtds.Data.Extensions;
using Oip.Rtds.HostedService;
using Oip.Rtds.Services;
using Oip.Rtds.Settings;

namespace Oip.Rtds;

internal static class Program
{
    public static void Main(string[] args)
    {
        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        try
        {
            var settings = AppSettings.Initialize(args, false, true);
            var builder = OipModuleApplication.CreateShellBuilder(settings);
            builder.Services.AddGrpc();
            builder.Services.AddSingleton<RtdsService>();
            builder.Services.AddSingleton<IRtdsAppSettings>(AppSettings.Instance);
            builder.Services.AddScoped<TagService>();
            builder.Services.AddRtdsData(settings);
            builder.Services.AddHostedService<RtdsHostedService>();
            var app = builder.BuildApp(settings);
            app.MapGrpcService<RtdsService>();
            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}