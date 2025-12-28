namespace ScreamReaderCore.Tools;

public sealed class CriticalSection : IDisposable
{
    private static readonly TimeSpan _defaultTimeToWaitForLock = TimeSpan.FromSeconds(1);
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    
    public async Task<Result<TResult>> EnterAsync<TResult>(Func<Task<Result<TResult>>> action, TimeSpan? timeToWaitFor = null, CancellationToken cancellationToken = default)
    {
        timeToWaitFor ??= _defaultTimeToWaitForLock;
        try
        {
            var success = await _lock.WaitAsync(timeToWaitFor.Value, cancellationToken);
            if (!success)
            {
                return Result.Failure<TResult>("Unable to enter critical section.");
            }
        }
        catch (Exception exception)
        {
            return Result.Failure<TResult>(exception);
        }

        try
        {
            var result = await action();
            return result;
        }
        catch (Exception exception)
        {
            return Result.Failure<TResult>(exception);
        }
        finally
        {
            _lock.Release();
        }
    }
    
    public async Task<Result> EnterAsync(Func<Task<Result>> action, TimeSpan? timeToWaitFor = null, CancellationToken cancellationToken = default)
    {
        timeToWaitFor ??= _defaultTimeToWaitForLock;
        try
        {
            var success = await _lock.WaitAsync(timeToWaitFor.Value, cancellationToken);
            if (!success)
            {
                return Result.Failure("Unable to enter critical section.");
            }
        }
        catch (Exception exception)
        {
            return Result.Failure(exception);
        }

        try
        {
            var result = await action();
            return result;
        }
        catch (Exception exception)
        {
            return Result.Failure(exception);
        }
        finally
        {
            _lock.Release();
        }
    }

    public Result Enter(Func<Result> action, TimeSpan? timeToWaitFor = null)
    {
        return EnterAsync(() =>
        {
            var result = action();
            return Task.FromResult(result);
        }, timeToWaitFor).GetAwaiter().GetResult();
    }
    
    public Result Enter(Action action, TimeSpan? timeToWaitFor = null)
    {
        return EnterAsync(() =>
        {
            action();
            return Task.FromResult(Result.Success());
        }, timeToWaitFor).GetAwaiter().GetResult();
    }
    
    public Result<TResult> Enter<TResult>(Func<Result<TResult>> action, TimeSpan? timeToWaitFor = null)
    {
        return EnterAsync<TResult>(() =>
        {
            var result = action();
            return Task.FromResult(result);
        }, timeToWaitFor).GetAwaiter().GetResult();
    }
    
    public void Dispose()
    {
        _lock.Dispose();
    }
}