using System.Net;
using System.Net.Sockets;
using Shouldly;

namespace ScreamReaderCore.Networking.Tests;

[TestFixture]
public class NetworkProviderTests
{
    private NetworkProvider _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new NetworkProvider();
    }
    
    [Test]
    public async Task Should_receive_udp_data()
    {
        using var socket = _sut.Open(SocketType.Udp, Data.Port).Value;
        var receiveTask = socket.ReceiveAsync();

        await SendUdpMessageAsync(Data.Localhost, Data.Port, Data.Message);
        var receiveResult = await receiveTask;
        
        receiveResult.IsSuccess.ShouldBeTrue();
        receiveResult.Value.ShouldBeEquivalentTo(Data.Message);
    }

    [Test]
    public void Should_not_throw_exception_when_communication_is_cancelled()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        using var socket = _sut.Open(SocketType.Udp, Data.Port).Value;
        
        var receiveTask = socket.ReceiveAsync(cancellationTokenSource.Token);
        cancellationTokenSource.Cancel();
        Should.NotThrowAsync(async () => await receiveTask);
        receiveTask.Result.IsSuccess.ShouldBeFalse();
    }
    
    private static async Task SendUdpMessageAsync(IPAddress address, int port, byte[] message)
    {
        using var udpClient = new UdpClient();
        await udpClient.SendAsync(message, new IPEndPoint(address, port));
    }
    
    private static class Data
    {
        public static readonly IPAddress Localhost = new IPAddress(new byte[] { 127, 0, 0, 1 });
        public const int Port = 35686;
        public static readonly byte[] Message = [0x01, 0x02, 0x03, 0x04, 0x05];
    }
}