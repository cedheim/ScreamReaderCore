using System.Net;
using FakeItEasy;
using ScreamReaderCore.Contract;
using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Tools;
using Shouldly;

namespace ScreamReaderCore.Lib.Tests;

public class ScreamPlayerTests
{
    private IPcmReceiver _receiver = null!;
    private IPcmOutput _output = null!;
    private PcmReceiverSettings _settings = null!;
    private ScreamPlayer _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _receiver = A.Fake<IPcmReceiver>();
        _output = A.Fake<IPcmOutput>();
        _settings = new PcmReceiverSettings(4010, IPAddress.Loopback, false);

        A.CallTo(() => _receiver.ReceiveAsync(A<CancellationToken>._))
            .ReturnsLazily((CancellationToken token) => WaitForCancellationAsync(token));

        _sut = new ScreamPlayer(_settings, _receiver, _output);
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
        _receiver.Dispose();
        _output.Dispose();
    }

    [Test]
    public void Should_not_be_playing_before_start()
    {
        _sut.IsPlaying.ShouldBeFalse();
    }

    [Test]
    public void Should_be_playing_after_start_and_not_after_stop()
    {
        _sut.Start();
        _sut.IsPlaying.ShouldBeTrue();

        _sut.Stop();
        _sut.IsPlaying.ShouldBeFalse();
    }

    [Test]
    public void Should_not_start_twice()
    {
        _sut.Start();
        _sut.Start();

        A.CallTo(() => _receiver.Open(_settings)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void Should_reopen_receiver_when_settings_change_while_playing()
    {
        var newSettings = _settings with { Port = 4011 };
        _sut.Start();

        _sut.Settings = newSettings;

        A.CallTo(() => _receiver.Close()).MustHaveHappenedOnceExactly()
            .Then(A.CallTo(() => _receiver.Open(newSettings)).MustHaveHappenedOnceExactly());
    }

    private static async Task<Result<PcmMessage>> WaitForCancellationAsync(CancellationToken token)
    {
        await Task.Delay(Timeout.Infinite, token);
        return Result.Failure<PcmMessage>("unreachable");
    }
}
