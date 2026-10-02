using NAudio.Wave;

namespace ScreamReaderCore.Audio;

/// <summary>
/// Wraps a <see cref="BufferedWaveProvider"/> and bounds how much audio can be queued, so latency cannot
/// creep up because of network bursts or the sender's clock running slightly faster than the sound card.
/// </summary>
/// <remarks>
/// Two trimming rules are applied, both discarding the oldest audio:
/// <list type="bullet">
/// <item>If the queue exceeds <see cref="AudioOutOptions.MaxBuffer"/> it is trimmed to <see cref="AudioOutOptions.TargetBuffer"/>.</item>
/// <item>If the lowest queue level seen during a <see cref="AudioOutOptions.TrimWindow"/> stayed above
/// <see cref="AudioOutOptions.TargetBuffer"/>, that standing excess was never needed and is discarded.</item>
/// </list>
/// <see cref="AddSamples"/> must be called from a single thread at a time; <see cref="Read"/> may run concurrently.
/// </remarks>
internal sealed class JitterBuffer : IWaveProvider
{
    private const int DiscardChunkSize = 4096;

    private readonly BufferedWaveProvider _buffer;
    private readonly int _targetBytes;
    private readonly int _maxBytes;
    private readonly int _trimWindowBytes;
    private readonly byte[] _discardBuffer;
    private int _windowMinBytes = int.MaxValue;
    private int _windowAddedBytes;

    public JitterBuffer(WaveFormat waveFormat, AudioOutOptions options)
    {
        ArgumentNullException.ThrowIfNull(waveFormat);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.TargetBuffer, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.MaxBuffer, options.TargetBuffer);

        _targetBytes = ToAlignedBytes(waveFormat, options.TargetBuffer);
        _maxBytes = ToAlignedBytes(waveFormat, options.MaxBuffer);
        _trimWindowBytes = ToAlignedBytes(waveFormat, options.TrimWindow);
        _discardBuffer = new byte[Math.Max(waveFormat.BlockAlign, DiscardChunkSize - DiscardChunkSize % waveFormat.BlockAlign)];
        _buffer = new BufferedWaveProvider(waveFormat)
        {
            // Only capacity; latency is bounded by the trimming rules.
            BufferLength = Math.Max(_maxBytes * 2, waveFormat.AverageBytesPerSecond),
            DiscardOnBufferOverflow = true,
            ReadFully = true
        };
    }

    public WaveFormat WaveFormat => _buffer.WaveFormat;

    public int BufferedBytes => _buffer.BufferedBytes;

    public TimeSpan BufferedDuration => _buffer.BufferedDuration;

    public long DiscardedBytes { get; private set; }

    public void AddSamples(byte[] buffer, int offset, int count)
    {
        // The queue level just before new data arrives is the low point since the previous packet.
        _windowMinBytes = Math.Min(_windowMinBytes, _buffer.BufferedBytes);
        _buffer.AddSamples(buffer, offset, count);
        _windowAddedBytes += count;

        var bufferedBytes = _buffer.BufferedBytes;
        if (bufferedBytes > _maxBytes)
        {
            Discard(bufferedBytes - _targetBytes);
            ResetWindow();
            return;
        }

        if (_windowAddedBytes < _trimWindowBytes)
        {
            return;
        }

        if (_windowMinBytes > _targetBytes)
        {
            Discard(_windowMinBytes - _targetBytes);
        }

        ResetWindow();
    }

    public int Read(byte[] buffer, int offset, int count) => _buffer.Read(buffer, offset, count);

    private void Discard(int bytes)
    {
        bytes -= bytes % WaveFormat.BlockAlign;
        DiscardedBytes += bytes;
        while (bytes > 0)
        {
            var chunk = Math.Min(bytes, _discardBuffer.Length);
            _buffer.Read(_discardBuffer, 0, chunk);
            bytes -= chunk;
        }
    }

    private void ResetWindow()
    {
        _windowMinBytes = int.MaxValue;
        _windowAddedBytes = 0;
    }

    private static int ToAlignedBytes(WaveFormat waveFormat, TimeSpan duration)
    {
        var bytes = (long)(duration.TotalSeconds * waveFormat.AverageBytesPerSecond);
        bytes = Math.Min(bytes, int.MaxValue / 4);
        return (int)(bytes - bytes % waveFormat.BlockAlign);
    }
}
