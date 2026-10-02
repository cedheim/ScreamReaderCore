using ScreamReaderCore.Contract;
using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Networking;
using ScreamReaderCore.Tools;
using SocketType = ScreamReaderCore.Networking.SocketType;

namespace ScreamReaderCore.Lib;

/// <summary>
/// Implements a PCM audio receiver using the ScreamReaderCore networking provider.
/// </summary>
public class ScreamReceiver : IPcmReceiver
{
    private readonly INetworkProvider _networkProvider;
    private INetworkSocket? _socket;
    private readonly CriticalSection _section = new();

    /// <summary>
    /// Creates a new ScreamReceiver instance.
    /// </summary>
    /// <param name="networkProvider"></param>
    public ScreamReceiver(INetworkProvider networkProvider)
    {
        _networkProvider = networkProvider;
    }
    
    /// <summary>
    /// Opens the receiver with the specified settings.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Closes the receiver.
    /// </summary>
    /// <returns></returns>
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
    
    /// <summary>
    /// Receives a PCM message asynchronously.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<PcmMessage>> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        // Only hold the lock while reading the socket reference; holding it across the receive
        // would block Close() until the next packet arrives.
        var socketResult = _section.Enter(() => _socket == null
            ? Result.Failure<INetworkSocket>("ScreamReceiver is not open.")
            : Result.Success(_socket));

        if (!socketResult.IsSuccess)
        {
            return Result.Failure<PcmMessage>(socketResult.Error);
        }

        var receiveResult = await socketResult.Value.ReceiveAsync(cancellationToken);

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

    /// <summary>
    /// Disposes the ScreamReceiver instance.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
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
