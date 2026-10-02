using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
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

    /// <summary>
    /// Raised (on a thread-pool thread) when the default render device for the multimedia role may have changed.
    /// </summary>
    event EventHandler? DefaultAudioEndpointChanged;
}

/// <summary>
/// Used for enumerating audio devices.
/// Wrapper for MMDeviceEnumerator from NAudio.
/// </summary>
[ExcludeFromCodeCoverage]
public class AudioDeviceEnumerator : IAudioDeviceEnumerator
{
    private readonly MMDeviceEnumerator _deviceEnumerator;
    private readonly NotificationClient _notificationClient;
    private volatile bool _disposed;

    public AudioDeviceEnumerator()
    {
        _deviceEnumerator = new MMDeviceEnumerator();
        // Kept in a field so the COM callable wrapper stays alive while registered.
        _notificationClient = new NotificationClient(this);
        _deviceEnumerator.RegisterEndpointNotificationCallback(_notificationClient);
    }

    /// <summary>
    /// Raised (on a thread-pool thread) when the default render device for the multimedia role may have changed.
    /// </summary>
    public event EventHandler? DefaultAudioEndpointChanged;
    
    /// <summary>
    /// Gets the default audio endpoint.
    /// </summary>
    /// <returns>The default audio device, null if none exists.</returns>
    public AudioDevice? GetDefaultAudioEndpoint()
    {
        try
        {
            using var defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return new AudioDevice(defaultDevice.ID, defaultDevice.FriendlyName, true);
        }
        catch (COMException)
        {
            // E_NOTFOUND when no render device exists.
            return null;
        }
    }
    
    /// <summary>
    /// Enumerates audio endpoints.
    /// </summary>
    /// <returns>Enumeration of audio devices.</returns>
    public IEnumerable<AudioDevice> EnumerateAudioEndPoints()
    {
        var defaultDeviceId = GetDefaultAudioEndpointId();
        var devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

        foreach (var device in devices)
        {
            using (device)
            {
                yield return new AudioDevice(
                    device.ID,
                    device.FriendlyName,
                    device.ID == defaultDeviceId
                );
            }
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
            using var device = _deviceEnumerator.GetDevice(id);
            if (device != null)
            {
                return new AudioDevice(device.ID, device.FriendlyName, device.ID == GetDefaultAudioEndpointId());
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
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _deviceEnumerator.UnregisterEndpointNotificationCallback(_notificationClient);
        _deviceEnumerator.Dispose();
    }

    /// <summary>
    /// Gets only the id of the default endpoint; much cheaper than reading the friendly name,
    /// which opens the device property store.
    /// </summary>
    private string? GetDefaultAudioEndpointId()
    {
        try
        {
            using var defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return defaultDevice.ID;
        }
        catch (COMException)
        {
            return null;
        }
    }

    private void RaiseDefaultAudioEndpointChanged()
    {
        if (_disposed)
        {
            return;
        }

        DefaultAudioEndpointChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Receives endpoint notifications from the Windows audio stack.
    /// Callbacks arrive on a system thread that must not block or call back into the device API,
    /// so work is handed off to the thread pool.
    /// </summary>
    private sealed class NotificationClient(AudioDeviceEnumerator owner) : IMMNotificationClient
    {
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            if (flow != DataFlow.Render || role != Role.Multimedia)
            {
                return;
            }

            ThreadPool.UnsafeQueueUserWorkItem(static o => o.RaiseDefaultAudioEndpointChanged(), owner, preferLocal: false);
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState) { }
        public void OnDeviceAdded(string pwstrDeviceId) { }
        public void OnDeviceRemoved(string deviceId) { }
        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }
    }
}