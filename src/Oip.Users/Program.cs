using NLog;
using NLog.Web;
using Oip.Base.Extensions;
using Oip.Example.Data.Extensions;
using Oip.Settings;

namespace Oip;

internal static class Program
{
    public static void Main(string[] args)
    {
        var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        try
        {
            var settings = AppSettings.Initialize(args, false, true);
            var builder = OipModuleApplication.CreateShellBuilder(settings);
            builder.Services.AddExampleDataContext(settings.ConnectionString);
            var app = builder.BuildApp(settings);
            app.Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Unhandled exception");
        }
    }
}