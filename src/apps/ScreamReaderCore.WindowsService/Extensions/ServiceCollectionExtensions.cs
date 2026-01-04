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
        services.AddTransient<INetworkProvider, NetworkProvider>();
        services.AddTransient<IAudioDeviceEnumerator, AudioDeviceEnumerator>();
        services.AddTransient<IAudioProvider, AudioProvider>(sp => new AudioProvider(sp.GetRequiredService<IAudioDeviceEnumerator>())
        {
            DeviceChangeMonitorInterval = TimeSpan.FromSeconds(1)
        });

        services.AddSingleton(_ => new PcmReceiverSettings(4010, new IPAddress([239, 255, 77, 77]), false));
        services.AddTransient<IPcmReceiver, ScreamReceiver>();
        services.AddTransient<IPcmOutput, ScreamOutput>();
        services.AddTransient<IScreamPlayer, ScreamPlayer>();
        
        return services;
    }
}