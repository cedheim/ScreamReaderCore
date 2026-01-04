using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace ScreamReaderCore.Audio;

public interface IAudioProvider : IDisposable
{
    IEnumerable<AudioDevice> GetAudioOutputDevices();
    AudioDevice? GetDefaultAudioOutputDevice();
    IAudioOut OpenOutput(AudioDevice device, int currentRate, int currentWidth, int currentChannels);
    event OnDefaultAudioDeviceChangedHandler OnDefaultDeviceChanged;
}

public delegate void OnDefaultAudioDeviceChangedHandler(AudioDevice newDefaultDevice);

public class AudioProvider : IAudioProvider
{
    private readonly MMDeviceEnumerator _deviceEnumerator;
    private readonly Thread _monitorThread;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public AudioProvider()
    {
        _deviceEnumerator = new MMDeviceEnumerator();
        _cancellationTokenSource = new CancellationTokenSource();
        _monitorThread = new Thread(() => MonitorDefaultDeviceChanges(_cancellationTokenSource.Token).Wait());
        _monitorThread.Start();
    }
    
    public AudioDevice? GetDefaultAudioOutputDevice()
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
    
    public event OnDefaultAudioDeviceChangedHandler? OnDefaultDeviceChanged;
    
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
        _cancellationTokenSource.Cancel();
        while (_monitorThread.IsAlive)
        {
            Thread.Sleep(10);
        }
        
        _deviceEnumerator.Dispose();
        
    }

    /// <summary>
    /// Workaround for default device change notifications not working reliably in NAudio.
    /// This method periodically checks the default device and raises an event if it changes.
    /// </summary>
    /// <param name="token">Cancellation token</param>
    private async Task MonitorDefaultDeviceChanges(CancellationToken token)
    {
        var defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        try
        {

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(100, token);
                var currentDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

                if (currentDevice.ID == defaultDevice.ID)
                {
                    continue;
                }

                OnDefaultDeviceChanged?.Invoke(new AudioDevice(currentDevice.ID, currentDevice.FriendlyName, true));
                defaultDevice = currentDevice;
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelling
        }
    }
}