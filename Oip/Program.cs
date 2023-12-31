using Oip.Base;
using Oip.Base.Extensions;
using Oip.Base.Settings;
using Oip.Settings;

namespace Oip;

internal static class Program
{
    public static void Main(string[] args)
    {
        var settings = BaseOipModuleAppSettings.Initialize(new AppSettingsOptions
        {
            UseEfCoreProvider = false,
            ProgrammeArguments = args,
        });
        var builder = OipModuleApplication.CreateModuleBuilder(settings);
        var app = builder.BuildModuleApp(settings);
        app.Run();
    }
}