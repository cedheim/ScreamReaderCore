using System.Collections.Concurrent;
using System.Net;
using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Tools;

namespace ScreamReaderCore.Networking;

/// <summary>
/// Provides network socket creation and management for supported socket types.
/// </summary>
public class NetworkProvider : INetworkProvider
{
    /// <summary>
    /// Opens a network socket of the specified type, port, and optional multicast address.
    /// </summary>
    /// <param name="type">The socket type.</param>
    /// <param name="port">The port to bind.</param>
    /// <param name="multicastAddress">Optional multicast address.</param>
    /// <returns>A result containing the network socket or an error.</returns>
    public Result<INetworkSocket> Open(SocketType type, int port, IPAddress? multicastAddress = null)
    {
        return type switch
        {
            SocketType.Udp => new UdpSocket(this, port, multicastAddress),
            _ => throw new NotSupportedException($"Socket type {type} is not supported.")
        };
    }
}