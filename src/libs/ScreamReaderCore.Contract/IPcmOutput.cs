using ScreamReaderCore.Audio;
using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Tools;

namespace ScreamReaderCore.Contract;

/// <summary>
/// Provides an interface for PCM audio output functionality.
/// </summary>
public interface IPcmOutput : IDisposable
{
    AudioDevice? Device { get; set; }
    
    Result Play(PcmMessage message);
}