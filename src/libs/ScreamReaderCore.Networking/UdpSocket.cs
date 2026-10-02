using System.Net;
using System.Net.Sockets;
using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Tools;

namespace ScreamReaderCore.Networking;

/// <summary>
/// Implements a UDP network socket for receiving data and managing resources.
/// </summary>
internal class UdpSocket : INetworkSocket
{
    private readonly NetworkProvider _provider;
    private readonly UdpClient _udpClient;

    /// <summary>
    /// Initializes a new instance of the UdpSocket class.
    /// </summary>
    /// <param name="provider">The network provider.</param>
    /// <param name="port">The port to bind.</param>
    /// <param name="multicastAddress">Optional multicast address.</param>
    public UdpSocket(NetworkProvider provider, int port, IPAddress? multicastAddress = null)
    {
        _provider = provider;
        _udpClient = new UdpClient { ExclusiveAddressUse = false };
        var localEp = new IPEndPoint(IPAddress.Any, port);

        _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _udpClient.Client.Bind(localEp);

        if (multicastAddress != null)
        {
            _udpClient.JoinMulticastGroup(multicastAddress);
        }
    }

    /// <summary>
    /// Receives data asynchronously from the UDP socket.
    /// A pending receive completes with a failure when the socket is disposed.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the received byte array or an error.</returns>
    public async Task<Result<byte[]>> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _udpClient.ReceiveAsync(cancellationToken);
            return Result.Success(result.Buffer);
        }
        catch (Exception e)
        {
            return Result.Failure<byte[]>(e);
        }
    }

    /// <summary>
    /// Disposes the UDP socket and releases resources.
    /// </summary>
    public void Dispose()
    {
        _udpClient.Dispose();
    }
}