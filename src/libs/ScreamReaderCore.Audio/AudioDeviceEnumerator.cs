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

[ExcludeFromCodeCoverage]
public class AudioDeviceEnumerator : IAudioDeviceEnumerator
{
    private readonly MMDeviceEnumerator _deviceEnumerator;

    public AudioDeviceEnumerator()
    {
        _deviceEnumerator = new MMDeviceEnumerator();
    }
    
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

    public bool HasDefaultAudioEndpoint()
    {
        return _deviceEnumerator.HasDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
    }

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

    public void Dispose()
    {
        _deviceEnumerator.Dispose();
    }
}