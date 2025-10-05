using System.Net;
using System.Net.Sockets;

namespace Oip.Base.Discovery;

public class NetHelper
{
    public static async Task<IPAddress> GetBroadcastAddressAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOCKER_ENV")))
            {
                return IPAddress.Parse("255.255.255.255");
            }

            var host = await Dns.GetHostEntryAsync(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    var addressBytes = ip.GetAddressBytes();
                    addressBytes[3] = 255; // Last octet to 255 for broadcast
                    return new IPAddress(addressBytes);
                }
            }
        }
        catch (Exception ex)
        {
            // TODO: Надо понять что делать
        }

        return IPAddress.Broadcast;
    }
}