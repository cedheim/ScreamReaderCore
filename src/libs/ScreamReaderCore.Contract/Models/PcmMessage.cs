namespace ScreamReaderCore.Contract.Models;

public sealed record PcmMessage
{
    public PcmMessage(byte[] data)
    {
        RawData = data ?? throw new ArgumentNullException(nameof(data));
        Header = new PcmHeader(data);
        Data = new ArraySegment<byte>(data, PcmHeader.HeaderSize, data.Length - PcmHeader.HeaderSize);
    }

    public byte[] RawData { get; }

    public ArraySegment<byte> Data { get;}

    public PcmHeader Header { get; }
}
