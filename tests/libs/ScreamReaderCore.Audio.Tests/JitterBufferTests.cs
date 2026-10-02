using NAudio.Wave;
using Shouldly;

namespace ScreamReaderCore.Audio.Tests;

public class JitterBufferTests
{
    // 48 kHz, 16-bit stereo: 192 bytes per millisecond. A Scream packet is 1152 bytes (6 ms).
    private static readonly WaveFormat Format = new(48000, 16, 2);
    private const int PacketMs = 6;

    private static readonly AudioOutOptions Options = new()
    {
        TargetBuffer = TimeSpan.FromMilliseconds(20),
        MaxBuffer = TimeSpan.FromMilliseconds(80),
        TrimWindow = TimeSpan.FromSeconds(1)
    };

    [Test]
    public void Should_not_trim_while_below_max_within_window()
    {
        var sut = new JitterBuffer(Format, Options);

        for (var i = 0; i < 10; i++)
        {
            AddPacket(sut, (byte)i);
        }

        sut.BufferedBytes.ShouldBe(Bytes(60));
        sut.DiscardedBytes.ShouldBe(0);
    }

    [Test]
    public void Should_discard_oldest_audio_down_to_target_when_max_is_exceeded()
    {
        var sut = new JitterBuffer(Format, Options);

        for (var i = 1; i <= 15; i++)
        {
            AddPacket(sut, (byte)i);
            sut.BufferedBytes.ShouldBeLessThanOrEqualTo(Bytes(80));
        }

        // Packet 14 pushed the queue to 84 ms, which was trimmed to the newest 20 ms
        // (2 ms of packet 11 + packets 12-14), then packet 15 was added.
        sut.BufferedBytes.ShouldBe(Bytes(26));
        var data = new byte[sut.BufferedBytes];
        sut.Read(data, 0, data.Length);
        data[0].ShouldBe((byte)11);
        data[Bytes(2)].ShouldBe((byte)12);
        data[^1].ShouldBe((byte)15);
    }

    [Test]
    public void Should_discard_standing_excess_after_a_full_window()
    {
        var sut = new JitterBuffer(Format, Options);
        var readBuffer = new byte[Bytes(PacketMs)];

        // A burst leaves 48 ms queued, which is below max, so only the window rule can remove it.
        for (var i = 0; i < 8; i++)
        {
            AddPacket(sut, 1);
        }

        var packetsPerWindow = (int)Math.Ceiling(Format.AverageBytesPerSecond / (double)Bytes(PacketMs));
        for (var i = 0; i < packetsPerWindow - 8; i++)
        {
            sut.Read(readBuffer, 0, readBuffer.Length);
            AddPacket(sut, 1);
        }

        // The first window started with an empty queue, so nothing is trimmed yet.
        sut.DiscardedBytes.ShouldBe(0);
        sut.BufferedBytes.ShouldBe(Bytes(48));

        for (var i = 0; i < packetsPerWindow * 2; i++)
        {
            sut.Read(readBuffer, 0, readBuffer.Length);
            AddPacket(sut, 1);
        }

        // Second window: the queue never dropped below 42 ms, so 42 - 20 = 22 ms was never needed.
        sut.DiscardedBytes.ShouldBe(Bytes(22));
        sut.BufferedBytes.ShouldBe(Bytes(26));
    }

    [Test]
    public void Should_keep_queue_block_aligned_when_target_is_not()
    {
        var options = Options with { TargetBuffer = TimeSpan.FromTicks(12345) };
        var sut = new JitterBuffer(Format, options);

        for (var i = 0; i < 15; i++)
        {
            AddPacket(sut, 1);
        }

        sut.DiscardedBytes.ShouldBeGreaterThan(0);
        (sut.BufferedBytes % Format.BlockAlign).ShouldBe(0);
        (sut.DiscardedBytes % Format.BlockAlign).ShouldBe(0);
    }

    [Test]
    public void Should_throw_when_max_is_below_target()
    {
        var options = Options with { MaxBuffer = TimeSpan.FromMilliseconds(10) };

        Should.Throw<ArgumentOutOfRangeException>(() => new JitterBuffer(Format, options));
    }

    private static void AddPacket(JitterBuffer buffer, byte value)
    {
        var packet = Enumerable.Repeat(value, Bytes(PacketMs)).ToArray();
        buffer.AddSamples(packet, 0, packet.Length);
    }

    private static int Bytes(int milliseconds) => Format.AverageBytesPerSecond / 1000 * milliseconds;
}
