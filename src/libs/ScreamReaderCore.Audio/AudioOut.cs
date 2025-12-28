using NAudio.CoreAudioApi;
using NAudio.Wave;
using ScreamReaderCore.Tools;

namespace ScreamReaderCore.Audio;

public interface IAudioOut : IDisposable
{
    Result Play(IReadOnlyCollection<byte> data);
    int Volume { get; set; }
}

internal class AudioOut : IAudioOut
{
    private readonly BufferedWaveProvider _waveProvider;
    private readonly WasapiOut _output;
    private readonly CriticalSection _lock = new CriticalSection();

    public AudioOut(MMDevice device, int currentRate, int currentWidth, int currentChannels)
    {
        var rate = ((currentRate >= 128) ? 44100 : 48000) * (currentRate % 128);
        _waveProvider = new BufferedWaveProvider(new WaveFormat(rate, currentWidth, currentChannels))
        {
            BufferDuration = TimeSpan.FromMilliseconds(200), DiscardOnBufferOverflow = true
        };
        _output = new WasapiOut(device, AudioClientShareMode.Shared, true, 200);
        _output.Init(_waveProvider);
        _output.Play();
    }

    public int Volume
    {
        get => (int)Math.Round(_output.Volume * 100);
        set
        {
            if (value is < 0 or > 100)
            {
                return;
            }

            _output.Volume = (float)value / 100f;
        }
    }

    public Result Play(IReadOnlyCollection<byte> data)
    {
        var dataArray = data as byte[] ?? data.ToArray();
        return _lock.Enter(() => _waveProvider.AddSamples(data.ToArray(), 0, data.Count));
    }

    public void Dispose()
    {
        var result = _lock.Enter(() =>
        {
            _output.Stop();
            _output.Dispose();
        });

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to dispose audio output with message: {result.Error}");
        }

        _lock.Dispose();
    }
}