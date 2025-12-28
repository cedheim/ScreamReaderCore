using System.Collections.Concurrent;
using System.Net;
using ScreamReaderCore.Contract.Models;

namespace ScreamReaderCore.Networking;

public class NetworkProvider : INetworkProvider
{
    private bool _disposed;
    private readonly HashSet<INetworkSocket> _sockets = new HashSet<INetworkSocket>();
    private readonly Semaphore _lock = new Semaphore(1, 1);
    
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
        _lock.WaitOne();
        _sockets.Add(socket);
        _lock.Release();
    }

    internal void Unregister(INetworkSocket socket)
    {
        _lock.WaitOne();
        _sockets.Remove(socket);
        _lock.Release();
    }

    public void Dispose()
    {
        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || _disposed)
        {
            return;
        }
        
        var unregistered = _sockets.ToArray();
        if (unregistered.Length > 0)
        {
            throw new InvalidOperationException("Unable to dispose NetworkProvider while there are active sockets.");
        }

        _lock.WaitOne();
        _lock.Release();
        _lock.Dispose();
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}