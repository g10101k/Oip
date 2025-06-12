using NLog;
using NLog.Web;
using Oip.Base.Extensions;
using Oip.Rts.Base.Contexts;
using Oip.Rts.Base.Settings;

namespace Oip.Rts;

internal static class Program
{
    public static void Main(string[] args)
    {
        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        try
        {
            var settings = AppSettings.Initialize(args, false, true);
            var builder = OipModuleApplication.CreateShellBuilder(settings);
            builder.Services.AddDbContext<RtdsMetaContext>(x =>
            {
            });
            builder.Services.AddScoped<RtdsContext>();
            builder.Services.AddSingleton(AppSettings.Instance);
            var app = builder.BuildApp(settings);
            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}