using NAudio.CoreAudioApi;

namespace ScreamReaderCore.Audio;

/// <summary>
/// Represents an audio device.
/// </summary>
/// <param name="Id">The id of the device</param>
/// <param name="Name">Friendly name</param>
/// <param name="IsDefault">Indicates that the device was default at the time of enumeration</param>
public record AudioDevice(string Id, string Name, bool IsDefault)
{
}