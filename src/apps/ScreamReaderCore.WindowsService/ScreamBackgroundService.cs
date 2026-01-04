using ScreamReaderCore.Audio;
using ScreamReaderCore.Lib;

namespace App.WindowsService;

public class ScreamBackgroundService : BackgroundService
{
    private readonly IScreamPlayer _screamPlayer;
    private readonly IAudioProvider _audioProvider;
    private readonly ILogger<ScreamBackgroundService> _logger;

    public ScreamBackgroundService(IScreamPlayer screamPlayer, IAudioProvider audioProvider,
        ILogger<ScreamBackgroundService> logger)
    {
        _screamPlayer = screamPlayer;
        _audioProvider = audioProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _audioProvider.OnDefaultDeviceChanged += (newDefaultDevice) =>
            {
                _screamPlayer.Device = newDefaultDevice;
                _logger.LogInformation("Audio device changed to: {Device}", newDefaultDevice.Name);
            };

            _screamPlayer.Start();
            _logger.LogInformation("ScreamBackgroundService started.");
            
            await Task.Delay(-1, stoppingToken);
        }
        catch (OperationCanceledException)
        {

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while starting the ScreamBackgroundService");
            
            Environment.Exit(1);
        }
        
        _screamPlayer.Stop();
    }
}