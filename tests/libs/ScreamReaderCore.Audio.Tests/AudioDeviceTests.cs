using Shouldly;

namespace ScreamReaderCore.Audio.Tests;

public class AudioDeviceTests
{
    [Test]
    public void Should_be_able_to_compare_audio_devices()
    {
        var device1 = new AudioDevice("id1", "Device 1", false);
        var device2 = new AudioDevice("id1", "Device 1", false);
        var device3 = new AudioDevice("id2", "Device 2", true);
        
        device1.Equals(device2).ShouldBeTrue();
        device1.Equals(device3).ShouldBeFalse();
        
        (device1 == device2).ShouldBeTrue();
        (device1 != device3).ShouldBeTrue();
    }
    
}