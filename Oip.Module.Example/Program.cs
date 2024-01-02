using Oip.Base.Extensions;
using Oip.Base.Settings;

namespace Oip.Module.Example;

internal static class Program
{
    public static void Main(string[] args)
    {
        var settings = BaseOipModuleAppSettings.Initialize(args, false, false);
        var builder = OipModuleApplication.CreateModuleBuilder(settings);
        var app = builder.BuildApp(settings);
        app.Run();
    }
}