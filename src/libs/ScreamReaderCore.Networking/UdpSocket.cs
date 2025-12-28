using System.Net;
using System.Net.Sockets;
using ScreamReaderCore.Contract.Models;

namespace ScreamReaderCore.Networking;

internal class UdpSocket : INetworkSocket
{
    private readonly NetworkProvider _provider;
    private bool _disposed;
    private readonly UdpClient _udpClient;
    private readonly Semaphore _lock;

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
        
        this._lock = new Semaphore(1, 1);
        this._provider.Register(this);
    }

    public async Task<Result<byte[]>> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            this._lock.WaitOne();
            var result = await _udpClient.ReceiveAsync(cancellationToken);
            return new Result<byte[]>(result.Buffer);
        }
        catch (Exception e)
        {
            return new Result<byte[]>(e);
        }
        finally{
            this._lock.Release();
        }
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

        _lock.WaitOne();
        _provider.Unregister(this);
        _udpClient?.Dispose();
        _lock.Release();
        _lock.Dispose();
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}