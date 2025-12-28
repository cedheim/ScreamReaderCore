using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Tools;

namespace ScreamReaderCore.Networking;

public interface INetworkSocket : IDisposable
{
    Task<Result<byte[]>> ReceiveAsync(CancellationToken cancellationToken = default);
}