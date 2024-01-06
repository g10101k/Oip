using Oip.Base.Extensions;
using Oip.Base.Settings;

namespace Oip;

internal static class Program
{
    public static void Main(string[] args)
    {
        var settings = BaseOipModuleAppSettings.Initialize(args, false, false);
        var builder = OipModuleApplication.CreateShellBuilder(settings);
        var app = builder.BuildApp(settings);
        app.Run();
    }
}