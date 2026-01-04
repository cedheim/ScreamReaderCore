using NUnit.Framework;
using Shouldly;
using FakeItEasy;
using ScreamReaderCore.Audio;
using ScreamReaderCore.Contract.Models;
using System;

namespace ScreamReaderCore.Lib.Tests;

public class ScreamOutputTests
{
    private IAudioProvider _provider = null!;
    private IAudioOut _audioOut = null!;
    private AudioDevice _device = null!;
    private ScreamOutput _sut = null!;
    private PcmHeader _header = null!;
    private PcmMessage _message = null!;

    [SetUp]
    public void SetUp()
    {
        _provider = A.Fake<IAudioProvider>();
        _audioOut = A.Fake<IAudioOut>();
        _device = new AudioDevice("id", "name", true);
        _message = new PcmMessage([1, 2, 3, 4, 5, 6, 7, 8]);
        _header = _message.Header;

        A.CallTo(() => _provider.GetDefaultAudioOutputDevice())
            .Returns(_device);
        A.CallTo(() => _provider.OpenOutput(A<AudioDevice>._, A<int>._, A<int>._, A<int>._))
            .Returns(_audioOut);

        _sut = new ScreamOutput(_provider);
    }

    [TearDown]
    public void TearDown()
    {
        _provider.Dispose();
        _audioOut.Dispose();
        _sut.Dispose();
    }

    [Test]
    public void Should_return_and_set_device_property()
    {
        _sut.Device.ShouldBe(_device);
        var newDevice = new AudioDevice("id2", "name2", false);

        _sut.Device = newDevice;
        _sut.Device.ShouldBe(newDevice);
    }

    [Test]
    public void Should_play_message_and_initialize_audio_out_on_header_change()
    {
        var newMessage = new PcmMessage([2, 2, 2, 2, 2, 2, 2, 2]);

        _sut.Play(newMessage);

        A.CallTo(() => _audioOut.Play(newMessage.Data)).MustHaveHappened();
    }

    [Test]
    public void Should_not_reinitialize_audio_out_if_header_unchanged()
    {
        _sut.Play(_message); // first call, initializes
        _sut.Play(_message); // second call, should not reinitialize

        A.CallTo(() =>
                _provider.OpenOutput(_device, _header.CurrentRate, _header.CurrentWidth, _header.CurrentChannels))
            .MustHaveHappened(1, Times.Exactly);
        A.CallTo(() => _audioOut.Play(_message.Data)).MustHaveHappened(2, Times.Exactly);
    }
}