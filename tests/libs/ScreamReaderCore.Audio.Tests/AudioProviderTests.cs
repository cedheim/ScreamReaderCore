using FakeItEasy;
using NAudio.CoreAudioApi;
using Shouldly;

namespace ScreamReaderCore.Audio.Tests;

public class AudioProviderTests
{
    private AudioProvider _sut = null!;
    private IAudioDeviceEnumerator _deviceEnumerator;

    [SetUp]
    public void SetUp()
    {
        _deviceEnumerator = A.Fake<IAudioDeviceEnumerator>();
        
        A.CallTo(() => _deviceEnumerator.EnumerateAudioEndPoints())
            .Returns([Data.SecondaryAudioDevice, Data.DefaultAudioDevice]);
        A.CallTo(() => _deviceEnumerator.GetDefaultAudioEndpoint())
            .Returns(Data.DefaultAudioDevice);
        A.CallTo(() => _deviceEnumerator.HasDefaultAudioEndpoint())
            .Returns(true);

        _sut = new AudioProvider(_deviceEnumerator);
    }
    
    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
        _deviceEnumerator.Dispose();
    }

    [Test]
    public void Should_return_at_least_one_audio_device()
    {
        var devices = _sut.GetAudioOutputDevices().ToList();
        
        devices.ShouldNotBeEmpty();
    }
    
    [Test]
    public void Should_be_able_to_get_default_audio_device()
    {
        var defaultDevice = _sut.GetDefaultAudioOutputDevice();
        var devices = _sut.GetAudioOutputDevices().ToList();
        var foundDefault = devices.First(d => d.IsDefault);
        
        defaultDevice.ShouldBeEquivalentTo(foundDefault);
    }
    
    [Test]
    public void Should_be_able_to_play_audio()
    {
        var device = GetDefaultAudioOutputDevice();
        
        using var audioOut = _sut.OpenOutput(device, 129, 16, 2);

        Should.NotThrow(() => audioOut.Play(Enumerable.Repeat((byte)40, 2000).ToArray()));
    }
    
    [Test]
    public void Should_be_able_to_set_volume()
    {
        var device = GetDefaultAudioOutputDevice();
        
        using var audioOut = _sut.OpenOutput(device, 129, 16, 2);

        var startVolume = audioOut.Volume;
        
        audioOut.Volume = 0.5f;
        audioOut.Volume.ShouldBe(0.5f, tolerance: 0.1);
        
        audioOut.Volume = 1.5f; // Invalid volume, should be ignored
        audioOut.Volume.ShouldBe(0.5f, tolerance: 0.1);
        
        audioOut.Volume = -1.0f; // Invalid volume, should be ignored
        audioOut.Volume.ShouldBe(0.5f, tolerance: 0.1);
        
        audioOut.Volume = 0.0f;
        audioOut.Volume.ShouldBe(0.0f, tolerance: 0.1);
        
        audioOut.Volume = 1.0f;
        audioOut.Volume.ShouldBe(1.0f, tolerance: 0.1);
        
        audioOut.Volume = startVolume;
    }
    
    [Test]
    public void Should_raise_event_when_default_device_changes()
    {
        var changes = new List<AudioDevice>();
        _sut.OnDefaultDeviceChanged += changes.Add;
        A.CallTo(() => _deviceEnumerator.GetDefaultAudioEndpoint())
            .Returns(Data.AnotherDefaultAudioDevice);

        _deviceEnumerator.DefaultAudioEndpointChanged += Raise.WithEmpty();

        changes.ShouldBe([Data.AnotherDefaultAudioDevice]);
    }

    [Test]
    public void Should_not_raise_event_when_default_device_is_unchanged()
    {
        var changes = new List<AudioDevice>();
        _sut.OnDefaultDeviceChanged += changes.Add;

        _deviceEnumerator.DefaultAudioEndpointChanged += Raise.WithEmpty();

        changes.ShouldBeEmpty();
    }

    [Test]
    public void Should_raise_event_once_for_duplicate_notifications()
    {
        var changes = new List<AudioDevice>();
        _sut.OnDefaultDeviceChanged += changes.Add;
        A.CallTo(() => _deviceEnumerator.GetDefaultAudioEndpoint())
            .Returns(Data.AnotherDefaultAudioDevice);

        _deviceEnumerator.DefaultAudioEndpointChanged += Raise.WithEmpty();
        _deviceEnumerator.DefaultAudioEndpointChanged += Raise.WithEmpty();

        changes.ShouldBe([Data.AnotherDefaultAudioDevice]);
    }

    [Test]
    public void Should_not_raise_event_when_no_default_device_exists()
    {
        var changes = new List<AudioDevice>();
        _sut.OnDefaultDeviceChanged += changes.Add;
        A.CallTo(() => _deviceEnumerator.GetDefaultAudioEndpoint())
            .Returns(null);

        _deviceEnumerator.DefaultAudioEndpointChanged += Raise.WithEmpty();

        changes.ShouldBeEmpty();
    }

    [Test]
    public void Should_not_raise_event_after_dispose()
    {
        var changes = new List<AudioDevice>();
        _sut.OnDefaultDeviceChanged += changes.Add;
        A.CallTo(() => _deviceEnumerator.GetDefaultAudioEndpoint())
            .Returns(Data.AnotherDefaultAudioDevice);

        _sut.Dispose();
        _deviceEnumerator.DefaultAudioEndpointChanged += Raise.WithEmpty();

        changes.ShouldBeEmpty();
    }

    private AudioDevice GetDefaultAudioOutputDevice()
    {
        using var deviceEnumerator = new MMDeviceEnumerator();
        var mmDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        return new AudioDevice(mmDevice.ID, mmDevice.FriendlyName, true);
    }

    private static class Data
    {
        public static readonly AudioDevice DefaultAudioDevice = new AudioDevice(Guid.NewGuid().ToString(), "Default Audio Device", true); 
        public static readonly AudioDevice SecondaryAudioDevice = new AudioDevice(Guid.NewGuid().ToString(), "Secondary Audio Device", false);
        public static readonly AudioDevice AnotherDefaultAudioDevice = new AudioDevice(Guid.NewGuid().ToString(), "Another Default Audio Device", true);
    }
}