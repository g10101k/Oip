using Oip.Settings;

namespace Oip.Rtds.Random.Settings;

public class AppSettings : BaseAppSettings<AppSettings>
{
    public string RtdsUrl { get; set; } = null!;

    public uint InterfaceId { get; set; }
}