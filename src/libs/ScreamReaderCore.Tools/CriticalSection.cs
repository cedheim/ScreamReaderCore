namespace ScreamReaderCore.Tools;

/// <summary>
/// Provides a thread-safe critical section for synchronizing access to resources.
/// </summary>
public sealed class CriticalSection : IDisposable
{
    private static readonly TimeSpan _defaultTimeToWaitForLock = TimeSpan.FromSeconds(1);
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    
    /// <summary>
    /// Enters the critical section asynchronously and executes the specified action.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="action">The asynchronous action to execute.</param>
    /// <param name="timeToWaitFor">Optional time to wait for the lock.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the outcome of the action.</returns>
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
    /// <summary>
    /// Enters the critical section asynchronously and executes the specified action.
    /// </summary>
    /// <param name="action">The asynchronous action to execute.</param>
    /// <param name="timeToWaitFor">Optional time to wait for the lock.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the outcome of the action.</returns>
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
    /// <summary>
    /// Enters the critical section and executes the specified function.
    /// </summary>
    /// <param name="action">The function to execute.</param>
    /// <param name="timeToWaitFor">Optional time to wait for the lock.</param>
    /// <returns>A result containing the outcome of the action.</returns>
    public Result Enter(Func<Result> action, TimeSpan? timeToWaitFor = null)
    {
        return EnterAsync(() =>
        {
            var result = action();
            return Task.FromResult(result);
        }, timeToWaitFor).GetAwaiter().GetResult();
    }
    /// <summary>
    /// Enters the critical section and executes the specified action.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="timeToWaitFor">Optional time to wait for the lock.</param>
    /// <returns>A result containing the outcome of the action.</returns>
    public Result Enter(Action action, TimeSpan? timeToWaitFor = null)
    {
        return EnterAsync(() =>
        {
            action();
            return Task.FromResult(Result.Success());
        }, timeToWaitFor).GetAwaiter().GetResult();
    }
    /// <summary>
    /// Enters the critical section and executes the specified function returning a result.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="action">The function to execute.</param>
    /// <param name="timeToWaitFor">Optional time to wait for the lock.</param>
    /// <returns>A result containing the outcome of the action.</returns>
    public Result<TResult> Enter<TResult>(Func<Result<TResult>> action, TimeSpan? timeToWaitFor = null)
    {
        return EnterAsync<TResult>(() =>
        {
            var result = action();
            return Task.FromResult(result);
        }, timeToWaitFor).GetAwaiter().GetResult();
    }
    /// <summary>
    /// Disposes the critical section and releases resources.
    /// </summary>
    public void Dispose()
    {
        _lock.Dispose();
    }
}