using ScreamReaderCore.Contract.Models;

namespace ScreamReaderCore.Networking;

public interface INetworkSocket : IDisposable
{
    Task<Result<byte[]>> ReceiveAsync(CancellationToken cancellationToken = default);
}