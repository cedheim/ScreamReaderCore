using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using ScreamReaderCore.Contract;
using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Networking;
using SocketType = ScreamReaderCore.Networking.SocketType;

namespace ScreamReaderCore.Lib;

public class ScreamReceiver : IPcmReceiver
{
    private readonly INetworkProvider _networkProvider;
    private bool _disposed;
    private INetworkSocket? _socket;
    private readonly Semaphore _lock = new Semaphore(1, 1);

    public ScreamReceiver(INetworkProvider networkProvider)
    {
        _networkProvider = networkProvider;
    }
    
    public Result Open(PcmReceiverSettings settings)
    {
        _lock.WaitOne();

        if (_socket != null)
        {
            _lock.Release();
            return new Result(false, "ScreamReceiver is already open.");
        }
        
        var socketResult = _networkProvider.Open(SocketType.Udp, settings.Port,
            settings.MulticastEnabled ? settings.MulticastAddress : null);

        if (socketResult.IsSuccess)
        {
            _socket = socketResult.Value;
        }
        
        _lock.Release();

        return socketResult;
    }

    public Result Close()
    {
        _lock.WaitOne();

        if (_socket == null)
        {
            _lock.Release();
            return new Result(false, "ScreamReceiver is not open.");
        }
        
        _socket.Dispose();
        _socket = null;
        _lock.Release();

        return new Result();
    }
    
    public async Task<Result<PcmMessage>> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        _lock.WaitOne();

        if (_socket == null)
        {
            _lock.Release();
            return new Result<PcmMessage>(false, "ScreamReceiver is not open.");
        }

        var receiveResult = await _socket.ReceiveAsync(cancellationToken);
        _lock.Release();

        if (!receiveResult.IsSuccess)
        {
            return new Result<PcmMessage>(false, receiveResult.Error);
        }

        try
        {
            var message = new PcmMessage(receiveResult);
            return message;
        }
        catch (Exception ex)
        {
            return new Result<PcmMessage>(ex);
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

        Close();
        
        _lock.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
