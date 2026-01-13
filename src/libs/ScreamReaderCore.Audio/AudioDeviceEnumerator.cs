using System.Diagnostics.CodeAnalysis;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace ScreamReaderCore.Audio;

/// <summary>
/// Provides an interface for enumerating and retrieving audio devices.
/// </summary>
public interface IAudioDeviceEnumerator : IDisposable
{
    AudioDevice? GetDefaultAudioEndpoint();
    IEnumerable<AudioDevice> EnumerateAudioEndPoints();
    bool HasDefaultAudioEndpoint();
    AudioDevice? GetDevice(string id);
    event OnDefaultAudioDeviceChangedHandler OnDefaultDeviceChanged;
}

/// <summary>
/// Used for enumerating audio devices.
/// Wrapper for MMDeviceEnumerator from NAudio.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioDeviceEnumerator : IAudioDeviceEnumerator
{
    private const DataFlow DefaultDataFlow = DataFlow.Render;
    private const Role DefaultRole = Role.Multimedia;
    
    private readonly MMDeviceEnumerator _deviceEnumerator;

    public AudioDeviceEnumerator()
    {
        _deviceEnumerator = new MMDeviceEnumerator();
        _deviceEnumerator.RegisterEndpointNotificationCallback(new AudioDeviceNotificationClient(this));
    }


    public event OnDefaultAudioDeviceChangedHandler? OnDefaultDeviceChanged;
    
    /// <summary>
    /// Gets the default audio endpoint.
    /// </summary>
    /// <returns>The default audio device, null if none exists.</returns>
    public AudioDevice? GetDefaultAudioEndpoint()
    {
        var defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DefaultDataFlow, DefaultRole);
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
        var defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DefaultDataFlow, DefaultRole);
        var devices = _deviceEnumerator.EnumerateAudioEndPoints(DefaultDataFlow, DeviceState.Active);

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
        return _deviceEnumerator.HasDefaultAudioEndpoint(DefaultDataFlow, DefaultRole);
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

    private class AudioDeviceNotificationClient : IMMNotificationClient
    {
        private readonly AudioDeviceEnumerator _enumerator;

        public AudioDeviceNotificationClient(AudioDeviceEnumerator enumerator)
        {
            _enumerator = enumerator;
        }
        
        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
        }

        public void OnDeviceAdded(string pwstrDeviceId)
        {
        }

        public void OnDeviceRemoved(string deviceId)
        {
        }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            if (flow != DefaultDataFlow || role != DefaultRole)
            {
                return;
            }
            
            new Thread(() =>
            {
                Thread.Sleep(100);
                var device = _enumerator.GetDevice(defaultDeviceId);
                _enumerator.OnDefaultDeviceChanged?.Invoke(device);
                
            }).Start();
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
        }
    }
}