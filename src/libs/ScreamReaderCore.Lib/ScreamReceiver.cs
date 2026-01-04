using ScreamReaderCore.Contract;
using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Networking;
using ScreamReaderCore.Tools;
using SocketType = ScreamReaderCore.Networking.SocketType;

namespace ScreamReaderCore.Lib;

public class ScreamReceiver : IPcmReceiver
{
    private readonly INetworkProvider _networkProvider;
    private INetworkSocket? _socket;
    private readonly CriticalSection _section = new();

    public ScreamReceiver(INetworkProvider networkProvider)
    {
        _networkProvider = networkProvider;
    }
    
    public Result Open(PcmReceiverSettings settings)
    {
        return _section.Enter(() =>
        {
            if (_socket != null)
            {
                return Result.Failure("ScreamReceiver is already open.");
            }

            var socketResult = _networkProvider.Open(SocketType.Udp, settings.Port,
                settings.MulticastEnabled ? settings.MulticastAddress : null);

            if (socketResult.IsSuccess)
            {
                _socket = socketResult.Value;
            }

            return socketResult;
        });
    }

    public Result Close()
    {
        return _section.Enter(() =>
        {
            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }

            return Result.Success();
        });
    }
    
    public async Task<Result<PcmMessage>> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        var receiveResult = await _section.EnterAsync(async () =>
        {
            if (_socket == null)
            {
                return Result.Failure<byte[]>("ScreamReceiver is not open.");
            }
            
            return await _socket.ReceiveAsync(cancellationToken);
        }, cancellationToken: cancellationToken);

        if (!receiveResult.IsSuccess)
        {
            return Result.Failure<PcmMessage>(receiveResult.Error);
        }

        try
        {
            var message = new PcmMessage(receiveResult);
            return message;
        }
        catch (Exception ex)
        {
            return Result.Failure<PcmMessage>(ex);
        }
    }

    public void Dispose()
    {
        var closeResult = Close();
        if (!closeResult.IsSuccess)
        {
            throw new InvalidOperationException("Unable to close ScreamReceiver while disposing.");
        }
        
        _section.Dispose();
    }
}
