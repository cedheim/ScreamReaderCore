using NAudio.CoreAudioApi;
using NAudio.Wave;
using ScreamReaderCore.Tools;

namespace ScreamReaderCore.Audio;

/// <summary>
/// Provides an interface for audio output functionality.
/// </summary>
public interface IAudioOut : IDisposable
{
    Result Play(ArraySegment<byte> data);
    float Volume { get; set; }
}

/// <summary>
/// Used for audio output.
/// </summary>
/// <remarks>
/// <see cref="Play"/> and <see cref="Dispose"/> must not be called concurrently; the caller is expected to serialize them.
/// </remarks>
internal class AudioOut : IAudioOut
{
    private readonly JitterBuffer _buffer;
    private readonly WasapiOut _output;
    private bool _disposed;

    /// <summary>
    /// Creates a new audio output instance.
    /// </summary>
    /// <param name="device">The audio device</param>
    /// <param name="currentRate"></param>
    /// <param name="currentWidth"></param>
    /// <param name="currentChannels"></param>
    /// <param name="options">Buffering options</param>
    public AudioOut(AudioDevice device, int currentRate, int currentWidth, int currentChannels, AudioOutOptions options)
    {
        var rate = ((currentRate >= 128) ? 44100 : 48000) * (currentRate % 128);
        using var deviceEnumerator = new MMDeviceEnumerator();
        
        var mmDevice = deviceEnumerator.GetDevice(device.Id);
        
        _buffer = new JitterBuffer(new WaveFormat(rate, currentWidth, currentChannels), options);
        _output = new WasapiOut(mmDevice, AudioClientShareMode.Shared, true,
            (int)Math.Ceiling(options.DeviceLatency.TotalMilliseconds));
        _output.Init(_buffer);
        _output.Play();
    }

    /// <summary>
    /// Volume of the output device (0.0 - 1.0).
    /// </summary>
    public float Volume
    {
        get => _output.Volume;
        set
        {
            if (value is < 0.0f or > 1.0f)
            {
                return;
            }

            _output.Volume = value;
        }
    }

    /// <summary>
    /// Plays audio data.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public Result Play(ArraySegment<byte> data)
    {
        if (_disposed)
        {
            return Result.Failure("Audio output is disposed.");
        }

        try
        {
            _buffer.AddSamples(data.Array!, data.Offset, data.Count);
            return Result.Success();
        }
        catch (Exception exception)
        {
            return Result.Failure(exception);
        }
    }

    /// <summary>
    /// Disposes the audio output.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _output.Stop();
        _output.Dispose();
    }
}