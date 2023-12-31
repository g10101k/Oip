using Oip.Base.Extensions;
using Oip.Base.Settings;

namespace Oip.Module;

internal static class Program
{
    public static void Main(string[] args)
    {
        var settings = BaseOipModuleAppSettings.Initialize(args, false, false);
        var builder = OipModuleApplication.CreateModuleBuilder(settings);
        var app = builder.BuildModuleApp(settings);
        app.Run();
    }
}