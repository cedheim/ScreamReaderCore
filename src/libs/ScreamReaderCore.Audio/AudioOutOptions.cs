namespace ScreamReaderCore.Audio;

/// <summary>
/// Buffering options for audio output. Lower values reduce latency but increase the risk of dropouts.
/// </summary>
public sealed record AudioOutOptions
{
    /// <summary>
    /// Default options, tuned for low latency on a LAN.
    /// </summary>
    public static AudioOutOptions Default { get; } = new();

    /// <summary>
    /// Size of the WASAPI device buffer. This buffer is always kept full, so it is a fixed part of the latency.
    /// </summary>
    public TimeSpan DeviceLatency { get; init; } = TimeSpan.FromMilliseconds(40);

    /// <summary>
    /// Minimum amount of queued audio to keep as a cushion against network jitter.
    /// Queued audio that stays above this level for a whole <see cref="TrimWindow"/> is discarded.
    /// </summary>
    public TimeSpan TargetBuffer { get; init; } = TimeSpan.FromMilliseconds(20);

    /// <summary>
    /// Hard cap for queued audio. When exceeded, the oldest audio is discarded down to <see cref="TargetBuffer"/>.
    /// </summary>
    public TimeSpan MaxBuffer { get; init; } = TimeSpan.FromMilliseconds(80);

    /// <summary>
    /// Window over which the minimum queue level is tracked before trimming standing latency.
    /// </summary>
    public TimeSpan TrimWindow { get; init; } = TimeSpan.FromSeconds(2);
}
