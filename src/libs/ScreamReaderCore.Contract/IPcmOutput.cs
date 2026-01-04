using ScreamReaderCore.Audio;
using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Tools;

namespace ScreamReaderCore.Contract;

public interface IPcmOutput : IDisposable
{
    AudioDevice? Device { get; set; }
    
    Result Play(PcmMessage message);
}