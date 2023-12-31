using Oip.Base;
using Oip.Base.Extensions;
using Oip.Base.Settings;
using Oip.Settings;

namespace Oip.Module;

internal static class Program
{
    public static void Main(string[] args)
    {
        var settings = BaseOipModuleAppSettings.Initialize(new AppSettingsOptions() { ProgrammeArguments = args,UseEfCoreProvider = false});
        var builder = OipModuleApplication.CreateModuleBuilder(settings);
        var app = builder.BuildModuleApp(settings);
        app.Run();
    }
}