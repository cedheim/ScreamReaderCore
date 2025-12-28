using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using ScreamReaderCore.Lib.Models;

namespace ScreamReaderCore.Lib;

public class ScreamReceiver
{
    private readonly UdpClient _udpClient;

    public ScreamReceiver()
    {
        _udpClient = new UdpClient { ExclusiveAddressUse = false };
    }
    
    public async Task<ScreamMessage> ReceiveAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
