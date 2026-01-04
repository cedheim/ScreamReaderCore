using Shouldly;

namespace ScreamReaderCore.Audio.Tests;

public class AudioProviderTests
{
    private AudioProvider _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new AudioProvider();
    }
    
    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
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
        var devices = _sut.GetAudioOutputDevices().ToList();
        var device = devices.First(d => d.IsDefault);
        
        using var audioOut = _sut.OpenOutput(device, 129, 16, 2);

        Should.NotThrow(() => audioOut.Play(Enumerable.Repeat((byte)40, 2000).ToArray()));
    }
    
    [Test]
    public void Should_be_able_to_set_volume()
    {
        var devices = _sut.GetAudioOutputDevices().ToList();
        var device = devices.First(d => d.IsDefault);
        
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
}