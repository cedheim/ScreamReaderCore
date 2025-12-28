using Shouldly;

namespace ScreamReaderCore.Tools.Tests;

public class CriticalSectionTests
{
    [Test]
    public void Should_enter_and_execute_action_synchronously()
    {
        using var section = new CriticalSection();
        var executed = false;
        var result = section.Enter(() => { executed = true; return Result.Success(); });
        result.IsSuccess.ShouldBeTrue();
        executed.ShouldBeTrue();
    }

    [Test]
    public void Should_return_failure_when_action_throws_synchronously()
    {
        using var section = new CriticalSection();
        var result = section.Enter(() => throw new InvalidOperationException());
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("InvalidOperationException");
    }

    [Test]
    public async Task Should_enter_and_execute_action_asynchronously()
    {
        using var section = new CriticalSection();
        var executed = false;
        var result = await section.EnterAsync(async () => { executed = true; await Task.Delay(10); return Result.Success(); });
        result.IsSuccess.ShouldBeTrue();
        executed.ShouldBeTrue();
    }

    [Test]
    public async Task Should_return_failure_when_action_throws_asynchronously()
    {
        using var section = new CriticalSection();
        var result = await section.EnterAsync(async () => { await Task.Yield(); throw new InvalidOperationException(); });
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("InvalidOperationException");
    }

    [Test]
    public void Should_timeout_when_lock_is_held()
    {
        using var section = new CriticalSection();
        var entered = new ManualResetEventSlim();
        var release = new ManualResetEventSlim();
        var t = Task.Run(() =>
        {
            // ReSharper disable once AccessToDisposedClosure
            section.Enter(() =>
            {
                entered.Set();
                release.Wait();
                return Result.Success();
            });
        });
        entered.Wait();
        var result = section.Enter(Result.Success, TimeSpan.FromMilliseconds(50));
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Unable to enter critical section");
        release.Set();
        t.Wait();
    }

    [Test]
    public async Task Should_timeout_when_lock_is_held_async()
    {
        using var section = new CriticalSection();
        var entered = new TaskCompletionSource();
        var release = new TaskCompletionSource();
        var t = Task.Run(async () =>
        {
            // ReSharper disable once AccessToDisposedClosure
            await section.EnterAsync(async () =>
            {
                entered.SetResult();
                await release.Task;
                return Result.Success();
            });
        });
        await entered.Task;
        var result = await section.EnterAsync(() => Task.FromResult(Result.Success()), TimeSpan.FromMilliseconds(50));
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Unable to enter critical section");
        release.SetResult();
        await t;
    }
}