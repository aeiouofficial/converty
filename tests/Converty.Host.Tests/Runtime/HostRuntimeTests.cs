using System.Runtime.Versioning;
using System.Security.Principal;
using Converty.Host.Ipc;
using Converty.Host.Jobs;
using Converty.Host.Runtime;

namespace Converty.Host.Tests.Runtime;

[SupportedOSPlatform("windows")]
public sealed class HostRuntimeTests
{
    private static readonly SecurityIdentifier RuntimeUser = new("S-1-5-21-111111111-222222222-333333333-2001");

    [Fact]
    public async Task SecondRuntimeForSameUserIsRejectedWhileFirstOwnsLease()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstRunner = new BlockingSessionRunner(entered, release);
        var first = new HostRuntime(RuntimeUser, CreateEmptyQueue, _ => firstRunner);
        using var firstCancellation = new CancellationTokenSource();
        Task<HostRuntimeResult> firstTask = first.RunAsync(firstCancellation.Token);
        await entered.Task.WaitAsync(TestContext.Current.CancellationToken);

        var second = new HostRuntime(RuntimeUser, CreateEmptyQueue, _ => new ImmediateSessionRunner());
        HostRuntimeResult secondResult = await second.RunAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HostRuntimeResult.AlreadyRunning, secondResult);
        release.TrySetResult();
        firstCancellation.Cancel();
        Assert.Equal(HostRuntimeResult.Stopped, await firstTask);
    }

    [Fact]
    public async Task QueueRecoveryHappensBeforeFirstPipeSession()
    {
        bool sessionCreated = false;
        var runtime = new HostRuntime(
            RuntimeUser,
            () => throw new InvalidDataException("corrupt journal"),
            _ =>
            {
                sessionCreated = true;
                return new ImmediateSessionRunner();
            });

        await Assert.ThrowsAsync<InvalidDataException>(
            () => runtime.RunAsync(TestContext.Current.CancellationToken));

        Assert.False(sessionCreated);
    }

    [Fact]
    public async Task CancellationStopsBoundedServerLoopAndReleasesLease()
    {
        using var cancellation = new CancellationTokenSource();
        var runner = new CancellingSessionRunner(cancellation);
        var runtime = new HostRuntime(RuntimeUser, CreateEmptyQueue, _ => runner);

        HostRuntimeResult result = await runtime.RunAsync(cancellation.Token);

        Assert.Equal(HostRuntimeResult.Stopped, result);
        Assert.Equal(1, runner.CallCount);
        Assert.True(HostSingleInstanceLease.TryAcquire(RuntimeUser, out HostSingleInstanceLease? reacquired));
        reacquired!.Dispose();
    }

    private static HostJobQueue CreateEmptyQueue() => new(capacity: 4);

    private sealed class ImmediateSessionRunner : IHostPipeSessionRunner
    {
        public Task<HostPipeSessionResult> RunSingleConnectionAsync(CancellationToken cancellationToken) =>
            Task.FromResult(HostPipeSessionResult.RequestHandled);
    }

    private sealed class BlockingSessionRunner(
        TaskCompletionSource entered,
        TaskCompletionSource release) : IHostPipeSessionRunner
    {
        public async Task<HostPipeSessionResult> RunSingleConnectionAsync(CancellationToken cancellationToken)
        {
            entered.TrySetResult();
            await release.Task.WaitAsync(cancellationToken);
            return HostPipeSessionResult.RequestHandled;
        }
    }

    private sealed class CancellingSessionRunner(CancellationTokenSource source) : IHostPipeSessionRunner
    {
        public int CallCount { get; private set; }

        public Task<HostPipeSessionResult> RunSingleConnectionAsync(CancellationToken cancellationToken)
        {
            CallCount++;
            source.Cancel();
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(HostPipeSessionResult.RequestHandled);
        }
    }
}
