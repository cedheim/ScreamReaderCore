using System;
using System.Collections.Generic;
using System.Text;

namespace ScreamReaderCore.Lib.Models;

public sealed record ScreamHeader
{
    public const int HeaderSize = 5;

    public ScreamHeader(byte[] data)
    {
        if (data.Length < HeaderSize)
        {
            throw new ArgumentException("Data length must be at least 5 bytes", nameof(data));
        }

        CurrentRate = data[0];
        CurrentWidth = data[1];
        CurrentChannels = data[2];
        CurrentChannelsMapLsb = data[3];
        CurrentChannelsMapMsb = data[4];
    }

    public int CurrentRate { get; }
    public int CurrentWidth { get; }
    public int CurrentChannels { get; }
    public int CurrentChannelsMapLsb { get; }
    public int CurrentChannelsMapMsb { get; }
    public int CurrentChannelsMap => (CurrentChannelsMapMsb << 8) | CurrentChannelsMapLsb;
}
