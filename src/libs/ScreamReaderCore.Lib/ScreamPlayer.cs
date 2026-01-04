using ScreamReaderCore.Audio;
using ScreamReaderCore.Contract;
using ScreamReaderCore.Contract.Models;
using ScreamReaderCore.Tools;

namespace ScreamReaderCore.Lib;

public interface IScreamPlayer : IDisposable
{
    AudioDevice? Device { get; set; }
    bool IsPlaying { get; }
    PcmReceiverSettings Settings { get; set; }
    void Start();
    void Stop();
}

public class ScreamPlayer : IScreamPlayer
{
    private readonly IPcmReceiver _receiver;
    private readonly IPcmOutput _output;
    
    
    private PcmReceiverSettings _settings;
    private Task<Result>? _playTask;
    private CancellationTokenSource? _cancellationTokenSource;

    public ScreamPlayer(PcmReceiverSettings settings, IPcmReceiver receiver, IPcmOutput output)
    {
        _settings = settings;
        _receiver = receiver;
        _output = output;
    }

    public AudioDevice? Device
    {
        get => _output.Device;
        set => _output.Device = value;
    }
    
    public bool IsPlaying => _playTask?.IsCompleted ?? false;
    
    public PcmReceiverSettings Settings 
    {
        get => _settings;
        set
        {
            _settings = value;
            if (!IsPlaying)
            {
                return;
            }

            _receiver.Close();
            _receiver.Open(_settings);
        }
    }
    
    public void Start()
    {
        if(IsPlaying)
        {
            return;
        }
        
        _cancellationTokenSource = new CancellationTokenSource();
        _playTask = PlayAsync(_cancellationTokenSource.Token);
    }

    public void Stop()
    {
        StopAsync().Wait();
    }

    public void Dispose()
    {
        if (IsPlaying)
        {
            Stop();
        }
    }

    private async Task StopAsync()
    {
        if (_cancellationTokenSource == null || _playTask == null || _playTask.IsCompleted)
        {
            return;
        }
        
        await _cancellationTokenSource.CancelAsync();
        await _playTask;
    }

    private async Task<Result> PlayAsync(CancellationToken cancellationToken)
    {
        try
        {
            _receiver.Open(_settings);

            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await _receiver.ReceiveAsync(cancellationToken);
                if (!message.IsSuccess)
                {
                    await Task.Delay(100);
                    continue;
                }

                _output.Play(message.Value);
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation
        }
        catch (Exception ex)
        {
            return Result.Failure(ex);
        }

        return Result.Success();
    }
}