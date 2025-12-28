using System;
using System.Collections.Generic;
using System.Text;

namespace ScreamReaderCore.Lib.Models;

public sealed record ScreamMessage
{
    public ScreamMessage(byte[] data)
    {
        RawData = data ?? throw new ArgumentNullException(nameof(data));
        Header = new ScreamHeader(data);
        Data = new ArraySegment<byte>(data, ScreamHeader.HeaderSize, data.Length - ScreamHeader.HeaderSize);
    }

    public byte[] RawData { get; }

    public ArraySegment<byte> Data { get;}

    public ScreamHeader Header { get; }
}
