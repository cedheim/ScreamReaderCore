using System.Net;

namespace ScreamReaderCore.Contract.Models;

public record PcmReceiverSettings(int Port, IPAddress MulticastAddress, bool MulticastEnabled)
{
}