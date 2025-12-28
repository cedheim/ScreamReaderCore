using ScreamReaderCore.Contract.Models;

namespace ScreamReaderCore.Contract;

public interface IPcmReceiver : IDisposable
{
    Result Open(PcmReceiverSettings settings);
    Result Close();
    Task<Result<PcmMessage>> ReceiveAsync(CancellationToken cancellationToken = default);
}