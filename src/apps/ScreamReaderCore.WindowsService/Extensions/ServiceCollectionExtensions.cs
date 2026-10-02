using System.Net;
using ScreamReaderCore.Audio;
using ScreamReaderCore.Contract;
using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Lib;
using ScreamReaderCore.Networking;

namespace App.WindowsService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScreamReader(this IServiceCollection services)
    {
        // Singletons: one device enumerator/notification registration and one audio pipeline per process.
        services.AddSingleton<INetworkProvider, NetworkProvider>();
        services.AddSingleton<IAudioDeviceEnumerator, AudioDeviceEnumerator>();
        services.AddSingleton(AudioOutOptions.Default);
        services.AddSingleton<IAudioProvider, AudioProvider>();

        services.AddSingleton(_ => new PcmReceiverSettings(4010, new IPAddress([239, 255, 77, 77]), false));
        services.AddSingleton<IPcmReceiver, ScreamReceiver>();
        services.AddSingleton<IPcmOutput, ScreamOutput>();
        services.AddSingleton<IScreamPlayer, ScreamPlayer>();
        
        return services;
    }
}