namespace ScreamReaderCore.Contract.Models;

public sealed record PcmMessage(byte[] RawData)
{
    public byte[] RawData { get; } = RawData ?? throw new ArgumentNullException(nameof(RawData));

    public ArraySegment<byte> Data { get;} = new(RawData, PcmHeader.HeaderSize, RawData.Length - PcmHeader.HeaderSize);

    public PcmHeader Header { get; } = new(RawData);
}
