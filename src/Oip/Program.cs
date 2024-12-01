using Oip.Base.Extensions;
using Oip.Base.Settings;
using Oip.Settings;

namespace Oip;

internal static class Program
{
    public static void Main(string[] args)
    {
        var settings = AppSettings.Initialize(args, false, true);
        var builder = OipModuleApplication.CreateShellBuilder(settings);
        var app = builder.BuildApp(settings);
        app.Run();
    }
}