using System.Net;
using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Tools;

namespace ScreamReaderCore.Networking;

public interface INetworkProvider : IDisposable
{
    Result<INetworkSocket> Open(SocketType type, int port, IPAddress? multicastAddress = null);
}