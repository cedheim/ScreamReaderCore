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
    private readonly IAudioDeviceEnumerator _deviceEnumerator;
    private readonly Thread _monitorThread;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public AudioProvider(IAudioDeviceEnumerator deviceEnumerator)
    {
        _deviceEnumerator = deviceEnumerator;
        _cancellationTokenSource = new CancellationTokenSource();
        _monitorThread = new Thread(() => MonitorDefaultDeviceChanges(_cancellationTokenSource.Token).Wait());
        _monitorThread.Start();
    }

    public TimeSpan DeviceChangeMonitorInterval { get; set; } = TimeSpan.FromMilliseconds(100);
    
    public AudioDevice? GetDefaultAudioOutputDevice() => _deviceEnumerator.GetDefaultAudioEndpoint();
    
    public event OnDefaultAudioDeviceChangedHandler? OnDefaultDeviceChanged;

    public IEnumerable<AudioDevice> GetAudioOutputDevices() => _deviceEnumerator.EnumerateAudioEndPoints();

    public IAudioOut OpenOutput(AudioDevice device, int currentRate, int currentWidth, int currentChannels)
    {
        return new AudioOut(device, currentRate, currentWidth, currentChannels);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        while (_monitorThread.IsAlive)
        {
            Thread.Sleep(10);
        }
    }

    /// <summary>
    /// Workaround for default device change notifications not working reliably in NAudio.
    /// This method periodically checks the default device and raises an event if it changes.
    /// </summary>
    /// <param name="token">Cancellation token</param>
    private async Task MonitorDefaultDeviceChanges(CancellationToken token)
    {
        var defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint();
        try
        {

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(DeviceChangeMonitorInterval, token);
                var currentDevice = _deviceEnumerator.GetDefaultAudioEndpoint();

                if (currentDevice == null || (defaultDevice != null && currentDevice.Id == defaultDevice.Id))
                {
                    continue;
                }

                defaultDevice = currentDevice;
                OnDefaultDeviceChanged?.Invoke(defaultDevice);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelling
        }
    }
}