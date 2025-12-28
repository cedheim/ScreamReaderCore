using System.Collections.Concurrent;
using System.Net;
using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Tools;

namespace ScreamReaderCore.Networking;

public class NetworkProvider : INetworkProvider
{
    private readonly HashSet<INetworkSocket> _sockets = new HashSet<INetworkSocket>();
    private readonly CriticalSection _section = new CriticalSection();
    
    public Result<INetworkSocket> Open(SocketType type, int port, IPAddress? multicastAddress = null)
    {
        return type switch
        {
            SocketType.Udp => new UdpSocket(this, port, multicastAddress),
            _ => throw new NotSupportedException($"Socket type {type} is not supported.")
        };
    }

    internal void Register(INetworkSocket socket)
    {
        _section.Enter(() => _sockets.Add(socket));
    }

    internal void Unregister(INetworkSocket socket)
    {
        _section.Enter(() => _sockets.Remove(socket));
    }

    public void Dispose()
    {
        var unregistered = _sockets.ToArray();
        if (unregistered.Length > 0)
        {
            throw new InvalidOperationException("Unable to dispose NetworkProvider while there are active sockets.");
        }
        
        _section.Dispose();
    }
}