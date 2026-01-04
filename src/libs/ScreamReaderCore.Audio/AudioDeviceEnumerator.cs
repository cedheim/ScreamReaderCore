using System.Diagnostics.CodeAnalysis;
using NAudio.CoreAudioApi;

namespace ScreamReaderCore.Audio;

public interface IAudioDeviceEnumerator : IDisposable
{
    AudioDevice? GetDefaultAudioEndpoint();
    IEnumerable<AudioDevice> EnumerateAudioEndPoints();
    bool HasDefaultAudioEndpoint();
    AudioDevice? GetDevice(string id);
}

/// <summary>
/// Used for enumerating audio devices.
/// Wrapper for MMDeviceEnumerator from NAudio.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioDeviceEnumerator : IAudioDeviceEnumerator
{
    private readonly MMDeviceEnumerator _deviceEnumerator;

    public AudioDeviceEnumerator()
    {
        _deviceEnumerator = new MMDeviceEnumerator();
    }
    
    /// <summary>
    /// Gets the default audio endpoint.
    /// </summary>
    /// <returns>The default audio device, null if none exists.</returns>
    public AudioDevice? GetDefaultAudioEndpoint()
    {
        var defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        if (defaultDevice == null)
        {
            return null;
        }
        
        return new AudioDevice(
            defaultDevice.ID,
            defaultDevice.FriendlyName,
            true
        );
    }
    
    /// <summary>
    /// Enumerates audio endpoints.
    /// </summary>
    /// <returns>Enumeration of audio devices.</returns>
    public IEnumerable<AudioDevice> EnumerateAudioEndPoints()
    {
        var defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        var devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

        foreach (var device in devices)
        {
            yield return new AudioDevice(
                device.ID,
                device.FriendlyName,
                device.ID == defaultDevice.ID
            );
        }
    }

    /// <summary>
    /// Indicates whether a default audio endpoint exists.
    /// </summary>
    /// <returns>True if default audio endpoint exists</returns>
    public bool HasDefaultAudioEndpoint()
    {
        return _deviceEnumerator.HasDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
    }

    /// <summary>
    /// Get audio device by id.
    /// </summary>
    /// <param name="id">Id device</param>
    /// <returns>The audio device, null if not found</returns>
    public AudioDevice? GetDevice(string id)
    {
        try
        {
            var defaultDevice = GetDefaultAudioEndpoint();
            var device = _deviceEnumerator.GetDevice(id);
            if (device != null)
            {
                return new AudioDevice(device.ID, device.FriendlyName, device.ID == defaultDevice?.Id);
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    /// <summary>
    /// Dispose the enumerator.
    /// </summary>
    public void Dispose()
    {
        _deviceEnumerator.Dispose();
    }
}