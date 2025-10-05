using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Oip.Base.Discovery;

public class FixedMulticastDiscoveryService : IDisposable
{
    private readonly UdpClient _udpClient;
    private readonly IPAddress _multicastAddress;
    private readonly int _multicastPort;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private bool _disposed = false;

    public FixedMulticastDiscoveryService(string multicastAddress = "239.255.255.255", int port = 8888)
    {
        _multicastAddress = IPAddress.Parse(multicastAddress);
        _multicastPort = port;
        _cancellationTokenSource = new CancellationTokenSource();

        _udpClient = new UdpClient();
        _udpClient.ExclusiveAddressUse = false;

        // Setting up multicast
        SetupMulticast();
    }

    private void SetupMulticast()
    {
        try
        {
            // More secure multicast setup
            _udpClient.JoinMulticastGroup(_multicastAddress);
            _udpClient.MulticastLoopback = true; // Enabling loopback for the local machine

            Console.WriteLine($"Multicast is set up on {_multicastAddress}:{_multicastPort}");
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.InvalidArgument)
        {
            Console.WriteLine(ex);
            // Using an alternative multicast setup method...
            Console.WriteLine("Using an alternative multicast setup method...");
            SetupMulticastAlternative();
        }
    }

    private void SetupMulticastAlternative()
    {
        try
        {
            // Creating a new socket with explicit settings
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // Binding to any address
            socket.Bind(new IPEndPoint(IPAddress.Any, _multicastPort));

            // Joining the multicast group
            var multicastOption = new MulticastOption(_multicastAddress);
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, multicastOption);

            // Using this socket for the UDP client
            _udpClient.Client = socket;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Alternative setup error: {ex}");
            throw;
        }
    }

    public async Task StartDiscoveryAsync()
    {
        Console.WriteLine("Starting the discovery service...");

        // Starting the listening in a separate task
        var listeningTask = StartListeningAsync(_cancellationTokenSource.Token);

        // Starting the announcement sending
        var announcingTask = StartAnnouncingAsync(_cancellationTokenSource.Token);

        await Task.WhenAll(listeningTask, announcingTask);
    }

    private async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        try
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Bind(new IPEndPoint(IPAddress.Any, _multicastPort));
            var multicastOption = new MulticastOption(_multicastAddress);
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, multicastOption);

            _udpClient.Client = socket;

            using var listener = new UdpClient();
            listener.Client = socket;

            listener.JoinMulticastGroup(_multicastAddress);

            Console.WriteLine($"Listening for multicast on port {_multicastPort}");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await listener.ReceiveAsync(cancellationToken);
                    var message = Encoding.UTF8.GetString(result.Buffer);

                    Console.WriteLine($"Message received: {message} from {result.RemoteEndPoint}");

                    // Processing the discovery message
                    HandleDiscoveryMessage(message, result.RemoteEndPoint);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while listening: {ex}");
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical listening error: {ex}");
        }
    }

    private async Task StartAnnouncingAsync(CancellationToken cancellationToken)
    {
        var serviceInfo = new
        {
            ServiceId = Guid.NewGuid(),
            MachineName = Environment.MachineName,
            Timestamp = DateTime.UtcNow,
            Type = "DiscoveryService"
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                serviceInfo = serviceInfo with { Timestamp = DateTime.UtcNow };
                var json = JsonSerializer.Serialize(serviceInfo);
                var data = Encoding.UTF8.GetBytes(json);

                await _udpClient.SendAsync(data, data.Length,
                    new IPEndPoint(_multicastAddress, _multicastPort));

                Console.WriteLine($"Announcement sent: {json}");

                await Task.Delay(5000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending announcement: {ex.Message}");
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    private void HandleDiscoveryMessage(string message, IPEndPoint remoteEndPoint)
    {
        try
        {
            var serviceInfo = JsonSerializer.Deserialize<dynamic>(message);
            Console.WriteLine($"Service discovered: {serviceInfo} on {remoteEndPoint}");

            // You can update the list of discovered services here
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
        }
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _cancellationTokenSource.Cancel();
            _udpClient?.Close();
            _udpClient?.Dispose();
            _cancellationTokenSource?.Dispose();
            _disposed = true;
        }
    }
}