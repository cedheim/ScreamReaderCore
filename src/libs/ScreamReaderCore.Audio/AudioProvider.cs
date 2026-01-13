using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace ScreamReaderCore.Audio;

/// <summary>
/// Provides an interface for audio device management and output creation.
/// </summary>
public interface IAudioProvider
{
    IEnumerable<AudioDevice> GetAudioOutputDevices();
    AudioDevice? GetDefaultAudioOutputDevice();
    IAudioOut OpenOutput(AudioDevice device, int currentRate, int currentWidth, int currentChannels);
    event OnDefaultAudioDeviceChangedHandler OnDefaultDeviceChanged;
}

/// <summary>
/// Used for providing audio devices and output.
/// </summary>
public class AudioProvider : IAudioProvider
{
    private readonly IAudioDeviceEnumerator _deviceEnumerator;

    /// <summary>
    /// Creates a new audio provider.
    /// </summary>
    /// <param name="deviceEnumerator"></param>
    public AudioProvider(IAudioDeviceEnumerator deviceEnumerator)
    {
        _deviceEnumerator = deviceEnumerator;
    }
    
    /// <summary>
    /// Gets the default audio output device.
    /// </summary>
    /// <returns></returns>
    public AudioDevice? GetDefaultAudioOutputDevice() => _deviceEnumerator.GetDefaultAudioEndpoint();

    /// <summary>
    /// Event raised when the default audio device changes.
    /// </summary>
    public event OnDefaultAudioDeviceChangedHandler? OnDefaultDeviceChanged 
    { 
        add => _deviceEnumerator.OnDefaultDeviceChanged += value; 
        remove => _deviceEnumerator.OnDefaultDeviceChanged -= value; 
    }

    /// <summary>
    /// Gets audio output devices.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<AudioDevice> GetAudioOutputDevices() => _deviceEnumerator.EnumerateAudioEndPoints();

    /// <summary>
    /// Crates an audio output instance.
    /// </summary>
    /// <param name="device"></param>
    /// <param name="currentRate"></param>
    /// <param name="currentWidth"></param>
    /// <param name="currentChannels"></param>
    /// <returns></returns>
    public IAudioOut OpenOutput(AudioDevice device, int currentRate, int currentWidth, int currentChannels)
    {
        return new AudioOut(device, currentRate, currentWidth, currentChannels);
    }
}