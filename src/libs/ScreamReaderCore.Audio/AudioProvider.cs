using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace ScreamReaderCore.Audio;

/// <summary>
/// Provides an interface for audio device management and output creation.
/// </summary>
public interface IAudioProvider : IDisposable
{
    IEnumerable<AudioDevice> GetAudioOutputDevices();
    AudioDevice? GetDefaultAudioOutputDevice();
    IAudioOut OpenOutput(AudioDevice device, int currentRate, int currentWidth, int currentChannels);
    event OnDefaultAudioDeviceChangedHandler OnDefaultDeviceChanged;
}

public delegate void OnDefaultAudioDeviceChangedHandler(AudioDevice newDefaultDevice);

/// <summary>
/// Used for providing audio devices and output.
/// </summary>
public class AudioProvider : IAudioProvider
{
    private readonly IAudioDeviceEnumerator _deviceEnumerator;
    private readonly AudioOutOptions _options;
    private readonly Lock _defaultDeviceLock = new();
    private string? _defaultDeviceId;
    private bool _disposed;

    /// <summary>
    /// Creates a new audio provider.
    /// </summary>
    /// <param name="deviceEnumerator"></param>
    /// <param name="options">Output buffering options, <see cref="AudioOutOptions.Default"/> if not specified.</param>
    public AudioProvider(IAudioDeviceEnumerator deviceEnumerator, AudioOutOptions? options = null)
    {
        _deviceEnumerator = deviceEnumerator;
        _options = options ?? AudioOutOptions.Default;
        _defaultDeviceId = _deviceEnumerator.GetDefaultAudioEndpoint()?.Id;
        _deviceEnumerator.DefaultAudioEndpointChanged += HandleDefaultAudioEndpointChanged;
    }
    
    /// <summary>
    /// Gets the default audio output device.
    /// </summary>
    /// <returns></returns>
    public AudioDevice? GetDefaultAudioOutputDevice() => _deviceEnumerator.GetDefaultAudioEndpoint();
    
    /// <summary>
    /// Event raised when the default audio device changes.
    /// </summary>
    public event OnDefaultAudioDeviceChangedHandler? OnDefaultDeviceChanged;

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
        return new AudioOut(device, currentRate, currentWidth, currentChannels, _options);
    }

    /// <summary>
    /// Disposes the audio provider.
    /// </summary>
    public void Dispose()
    {
        _deviceEnumerator.DefaultAudioEndpointChanged -= HandleDefaultAudioEndpointChanged;
        lock (_defaultDeviceLock)
        {
            _disposed = true;
        }
    }

    /// <summary>
    /// Re-queries the default device instead of trusting the notification payload, and serializes handling,
    /// so out-of-order or duplicate notifications (Windows sends several per change) raise at most one event
    /// and the last raised device is always the current default.
    /// </summary>
    private void HandleDefaultAudioEndpointChanged(object? sender, EventArgs e)
    {
        lock (_defaultDeviceLock)
        {
            if (_disposed)
            {
                return;
            }

            AudioDevice? currentDevice;
            try
            {
                currentDevice = _deviceEnumerator.GetDefaultAudioEndpoint();
            }
            catch
            {
                return;
            }

            if (currentDevice == null || currentDevice.Id == _defaultDeviceId)
            {
                return;
            }

            _defaultDeviceId = currentDevice.Id;
            OnDefaultDeviceChanged?.Invoke(currentDevice);
        }
    }
}