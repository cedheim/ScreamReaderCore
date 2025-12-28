using FakeItEasy;
using ScreamReaderCore.Networking;
using ScreamReaderCore.Contract.Models;
using System.Net;
using ScreamReaderCore.Tools;
using Shouldly;

namespace ScreamReaderCore.Lib.Tests;

public class ScreamReceiverTests
{
    private INetworkProvider _provider = null!;
    private ScreamReceiver _sut = null!;
    private INetworkSocket _socket = null!;
    private PcmReceiverSettings _settings = null!;

    [SetUp]
    public void SetUp()
    {
        _provider = A.Fake<INetworkProvider>();
        _socket = A.Fake<INetworkSocket>();
        _settings = new PcmReceiverSettings(1234, IPAddress.Loopback, false);
        _sut = new ScreamReceiver(_provider);
    }

    [TearDown]
    public void TearDown()
    {
        _socket?.Dispose();
        _provider.Dispose();
        _sut.Dispose();
    }

    [Test]
    public void Should_open_when_not_open_returns_success_and_opens_socket()
    {
        A.CallTo(() => _provider.Open(SocketType.Udp, _settings.Port, null))
            .Returns(Result.Success(_socket));

        var result = _sut.Open(_settings);

        result.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public void Should_open_when_already_open_returns_error()
    {
        A.CallTo(() => _provider.Open(SocketType.Udp, _settings.Port, null))
            .Returns(Result.Success(_socket));
        _sut.Open(_settings);
        var result = _sut.Open(_settings);
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("already open");
    }

    [Test]
    public void Should_close_when_not_open_success()
    {
        var result = _sut.Close();
        result.IsSuccess.ShouldBeTrue();
    }

    [Test]
    public void Should_close_when_open_disposes_socket_and_returns_success()
    {
        A.CallTo(() => _provider.Open(SocketType.Udp, _settings.Port, null))
            .Returns(Result.Success(_socket));
        _sut.Open(_settings);
        var result = _sut.Close();
        result.IsSuccess.ShouldBeTrue();
        A.CallTo(() => _socket.Dispose()).MustHaveHappened();
    }

    [Test]
    public async Task Should_receive_async_when_not_open_returns_error()
    {
        var result = await _sut.ReceiveAsync();
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("not open");
    }

    [Test]
    public async Task Should_receive_async_when_socket_returns_error_returns_error()
    {
        A.CallTo(() => _provider.Open(SocketType.Udp, _settings.Port, null))
            .Returns(Result.Success(_socket));
        _sut.Open(_settings);
        A.CallTo(() => _socket.ReceiveAsync(A<CancellationToken>._))
            .Returns(Result.Failure<byte[]>("fail"));
        var result = await _sut.ReceiveAsync();
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("fail");
    }

    [Test]
    public async Task Should_receive_async_when_socket_returns_data_returns_pcm_message()
    {
        A.CallTo(() => _provider.Open(SocketType.Udp, _settings.Port, null))
            .Returns(Result.Success(_socket));
        _sut.Open(_settings);
        var data = new byte[] { 1, 2, 3, 4, 5, 6, 7 };
        A.CallTo(() => _socket.ReceiveAsync(A<CancellationToken>._))
            .Returns(Result.Success(data));
        var result = await _sut.ReceiveAsync();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.RawData.ShouldBe(data);
    }
}