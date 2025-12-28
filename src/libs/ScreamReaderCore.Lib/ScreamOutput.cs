using ScreamReaderCore.Audio;
using ScreamReaderCore.Contract;
using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Tools;

namespace ScreamReaderCore.Lib;

public class ScreamOutput : IPcmOutput
{
    private readonly IAudioProvider _provider;
    private readonly CriticalSection _section = new();
    
    private AudioDevice _audioDevice;
    private PcmHeader? _pcmHeader;
    private IAudioOut? _audioOut;

    public ScreamOutput(IAudioProvider provider)
    {
        _provider = provider;
        _audioDevice = _provider.GetDefaultAudioOutputDevice();
    }

    public AudioDevice Device
    {
        get
        {
            return _audioDevice;
        }
        set
        {
            _audioDevice = value;
            
            EnsureAudioOutInitialized();
        }
    }
    
    
    public Result Play(PcmMessage message)
    {
        return _section.Enter(() =>
        {
            if(_pcmHeader == null || _pcmHeader != message.Header)
            {
                _pcmHeader = message.Header;
                EnsureAudioOutInitialized();
            }

            _audioOut?.Play(message.Data);
        });
    }
    
    private void EnsureAudioOutInitialized()
    {
        if (_pcmHeader == null)
        {
            return;
        }
        
        _audioOut?.Dispose();
        _audioOut = _provider.OpenOutput(
            _audioDevice,
            _pcmHeader.CurrentRate,
            _pcmHeader.CurrentWidth,
            _pcmHeader.CurrentChannels
        );
    }
    
    public void Dispose()
    {
        var result = _section.Enter(() =>
        {
            _audioOut?.Dispose();
            _provider.Dispose();
        });

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Unable to dispose of ScreamOutput: {result.Error}");
        }
        
        _section.Dispose();
    }
}