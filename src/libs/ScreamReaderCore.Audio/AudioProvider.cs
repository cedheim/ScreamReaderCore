using NAudio.CoreAudioApi;

namespace ScreamReaderCore.Audio;

public interface IAudioProvider : IDisposable
{
    IEnumerable<AudioDevice> GetAudioOutputDevices();
    IAudioOut OpenOutput(AudioDevice device, int currentRate, int currentWidth, int currentChannels);
}

public class AudioProvider : IAudioProvider
{
    private readonly MMDeviceEnumerator _deviceEnumerator;

    public AudioProvider()
    {
        _deviceEnumerator = new MMDeviceEnumerator();
    }
    
    public IEnumerable<AudioDevice> GetAudioOutputDevices()
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

    public IAudioOut OpenOutput(AudioDevice device, int currentRate, int currentWidth, int currentChannels)
    {
        var mmDevice = _deviceEnumerator.GetDevice(device.Id);
        
        return new AudioOut(mmDevice, currentRate, currentWidth, currentChannels);
    }

    public void Dispose()
    {
        _deviceEnumerator.Dispose();
    }
}