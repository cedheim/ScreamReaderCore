using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Tools;

namespace ScreamReaderCore.Contract;

public interface IPcmReceiver : IDisposable
{
    Result Open(PcmReceiverSettings settings);
    Result Close();
    Task<Result<PcmMessage>> ReceiveAsync(CancellationToken cancellationToken = default);
}