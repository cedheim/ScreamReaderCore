using NAudio.CoreAudioApi;

namespace ScreamReaderCore.Audio;

public record AudioDevice(string Id, string Name, bool IsDefault)
{
}