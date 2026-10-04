using Oip.Base.Settings;
using Oip.Settings;

namespace Oip.Rtds.RandomInterface.Settings;

public class AppSettings : BaseAppSettings<AppSettings>
{
    public string RtdsUrl { get; set; } = null!;

    public uint InterfaceId { get; set; }

    /// <summary>
    /// Keycloak client whose service account authorizes calls to the RTDS gRPC service.
    /// </summary>
    public SecurityServiceSettings SecurityService { get; set; } = new();
}
