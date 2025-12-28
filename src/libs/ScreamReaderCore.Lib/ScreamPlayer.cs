using System.Net;
using ScreamReaderCore.Contract;
using ScreamReaderCore.Contract.Models;

namespace ScreamReaderCore.Lib;

public class ScreamPlayer
{
    private readonly IPcmReceiver _receiver;
    private readonly IPcmOutput _output;

    public ScreamPlayer(IPcmReceiver receiver, IPcmOutput output)
    {
        _receiver = receiver;
        _output = output;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var settings = new PcmReceiverSettings(4010, new IPAddress([239, 255, 77, 77]), false);
        _receiver.Open(settings);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var message = await _receiver.ReceiveAsync(cancellationToken);
            if (!message.IsSuccess)
            {
                await Task.Delay(100);
                continue;
            }
            
            _output.Play(message.Value);
        }
    }
}