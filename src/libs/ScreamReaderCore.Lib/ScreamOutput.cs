using ScreamReaderCore.Audio;
using ScreamReaderCore.Contract;
using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Tools;

namespace ScreamReaderCore.Lib;

/// <summary>
/// Implements PCM audio output using the ScreamReaderCore audio provider.
/// </summary>
public class ScreamOutput : IPcmOutput
{
    private readonly IAudioProvider _provider;
    private readonly CriticalSection _section = new();
    
    private AudioDevice? _audioDevice;
    private PcmHeader? _pcmHeader;
    private IAudioOut? _audioOut;

    /// <summary>
    /// Creates a new ScreamOutput instance.
    /// </summary>
    /// <param name="provider"></param>
    public ScreamOutput(IAudioProvider provider)
    {
        _provider = provider;
        _audioDevice = _provider.GetDefaultAudioOutputDevice();
    }

    /// <summary>
    /// The audio device used for output.
    /// </summary>
    public AudioDevice? Device
    {
        get
        {
            return _audioDevice;
        }
        set
        {
            // Device changes arrive on a thread-pool thread; serialize with Play so the output
            // is never disposed/recreated while a packet is being queued.
            _section.Enter(() =>
            {
                _audioDevice = value;
                EnsureAudioOutInitialized();
            });
        }
    }
    
    /// <summary>
    /// Plays a PCM message.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public Result Play(PcmMessage message)
    {
        return _section.Enter(() =>
        {
            if(_pcmHeader == null || _pcmHeader != message.Header)
            {
                _pcmHeader = message.Header;
                EnsureAudioOutInitialized();
            }

            return _audioOut?.Play(message.Data) ?? Result.Failure("No audio output available.");
        });
    }
    
    /// <summary>
    /// Ensures that the audio output is initialized.
    /// </summary>
    private void EnsureAudioOutInitialized()
    {
        if (_pcmHeader == null || _audioDevice == null)
        {
            return;
        }
        
        _audioOut?.Dispose();
        _audioOut = null;
        _audioOut = _provider.OpenOutput(
            _audioDevice,
            _pcmHeader.CurrentRate,
            _pcmHeader.CurrentWidth,
            _pcmHeader.CurrentChannels
        );
    }
    
    /// <summary>
    /// Disposes the ScreamOutput instance.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Dispose()
    {
        var result = _section.Enter(() =>
        {
            _audioOut?.Dispose();
        });

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Unable to dispose of ScreamOutput: {result.Error}");
        }
        
        _section.Dispose();
    }
}