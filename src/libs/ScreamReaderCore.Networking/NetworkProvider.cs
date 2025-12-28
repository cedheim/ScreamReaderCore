using System.Collections.Concurrent;
using System.Net;
using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Tools;

namespace ScreamReaderCore.Networking;

public class NetworkProvider : INetworkProvider
{
    public Result<INetworkSocket> Open(SocketType type, int port, IPAddress? multicastAddress = null)
    {
        return type switch
        {
            SocketType.Udp => new UdpSocket(this, port, multicastAddress),
            _ => throw new NotSupportedException($"Socket type {type} is not supported.")
        };
    }
}