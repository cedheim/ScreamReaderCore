using System.Net;
using System.Net.Sockets;
using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Tools;

namespace ScreamReaderCore.Networking;

internal class UdpSocket : INetworkSocket
{
    private readonly NetworkProvider _provider;
    private readonly UdpClient _udpClient;
    private readonly CriticalSection _section;

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
        
        this._section = new CriticalSection();
        this._provider.Register(this);
    }

    public async Task<Result<byte[]>> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        return await _section.EnterAsync(async () =>
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
        }, cancellationToken: cancellationToken);
    }

    public void Dispose()
    {
        var result = _section.Enter(() =>
        {
            _provider.Unregister(this);
            _udpClient?.Dispose();
        });

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to dispose UdpSocket with message: {result.Error}");
        }
        
        _section.Dispose();
    }
}