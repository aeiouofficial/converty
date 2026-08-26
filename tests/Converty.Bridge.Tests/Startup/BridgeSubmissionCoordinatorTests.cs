using Converty.Bridge.Ipc;
using Converty.Bridge.Startup;
using Converty.Contracts;
using Converty.Contracts.Conversion;

namespace Converty.Bridge.Tests.Startup;

public sealed class BridgeSubmissionCoordinatorTests
{
    [Fact]
    public async Task SuccessfulFirstSubmissionNeverLaunchesHost()
    {
        var client = new SequenceClient(new BridgeSubmissionResult(true, Guid.NewGuid(), null));
        var launcher = new CountingLauncher();
        var coordinator = CreateCoordinator(client, launcher);

        BridgeSubmissionResult result = await coordinator.SubmitAsync(CreateRequest(), TestContext.Current.CancellationToken);

        Assert.True(result.Accepted);
        Assert.Equal(0, launcher.StartCount);
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public async Task RejectedFirstSubmissionNeverLaunchesHost()
    {
        var client = new SequenceClient(new BridgeSubmissionResult(false, null, "rejected"));
        var launcher = new CountingLauncher();
        var coordinator = CreateCoordinator(client, launcher);

        BridgeSubmissionResult result = await coordinator.SubmitAsync(CreateRequest(), TestContext.Current.CancellationToken);

        Assert.False(result.Accepted);
        Assert.Equal(0, launcher.StartCount);
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public async Task ProtocolFailureNeverLaunchesHost()
    {
        var client = new SequenceClient(new InvalidDataException("bad response"));
        var launcher = new CountingLauncher();
        var coordinator = CreateCoordinator(client, launcher);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => coordinator.SubmitAsync(CreateRequest(), TestContext.Current.CancellationToken));

        Assert.Equal(0, launcher.StartCount);
        Assert.Equal(1, client.CallCount);
    }

    [Fact]
    public async Task UnavailableHostLaunchesExactlyOnceThenReturnsRetryResult()
    {
        var expected = new BridgeSubmissionResult(true, Guid.NewGuid(), null);
        var client = new SequenceClient(new BridgeHostUnavailableException("offline"), expected);
        var launcher = new CountingLauncher();
        var coordinator = CreateCoordinator(client, launcher);

        BridgeSubmissionResult result = await coordinator.SubmitAsync(CreateRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(expected, result);
        Assert.Equal(1, launcher.StartCount);
        Assert.Equal(2, client.CallCount);
    }

    [Fact]
    public async Task RepeatedUnavailabilityTimesOutWithoutSecondLaunch()
    {
        var client = new AlwaysUnavailableClient();
        var launcher = new CountingLauncher();
        var coordinator = new BridgeSubmissionCoordinator(
            client,
            launcher,
            startupTimeout: TimeSpan.FromMilliseconds(100),
            retryDelay: TimeSpan.FromMilliseconds(10));

        await Assert.ThrowsAsync<TimeoutException>(
            () => coordinator.SubmitAsync(CreateRequest(), TestContext.Current.CancellationToken));

        Assert.Equal(1, launcher.StartCount);
        Assert.True(client.CallCount >= 2);
    }

    [Fact]
    public async Task CallerCancellationStopsStartupRetryPromptly()
    {
        var client = new AlwaysUnavailableClient();
        var launcher = new CountingLauncher();
        var coordinator = new BridgeSubmissionCoordinator(
            client,
            launcher,
            startupTimeout: TimeSpan.FromSeconds(2),
            retryDelay: TimeSpan.FromMilliseconds(50));
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.CancelAfter(TimeSpan.FromMilliseconds(75));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => coordinator.SubmitAsync(CreateRequest(), cancellation.Token));

        Assert.Equal(1, launcher.StartCount);
    }

    private static BridgeSubmissionCoordinator CreateCoordinator(IBridgeRequestClient client, IHostProcessLauncher launcher) =>
        new(client, launcher, startupTimeout: TimeSpan.FromSeconds(1), retryDelay: TimeSpan.FromMilliseconds(10));

    private static ConversionRequest CreateRequest() =>
        new(
            SchemaVersions.Current,
            Guid.NewGuid(),
            ConversionAction.ConvertUsingDefault,
            [@"C:\input\sample.wav"],
            targetFormat: null,
            presetId: null);

    private sealed class CountingLauncher : IHostProcessLauncher
    {
        public int StartCount { get; private set; }

        public void StartHost() => StartCount++;
    }

    private sealed class SequenceClient(params object[] outcomes) : IBridgeRequestClient
    {
        private readonly Queue<object> _outcomes = new(outcomes);

        public int CallCount { get; private set; }

        public Task<BridgeSubmissionResult> SubmitAsync(ConversionRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            object outcome = _outcomes.Dequeue();
            return outcome switch
            {
                BridgeSubmissionResult result => Task.FromResult(result),
                Exception error => Task.FromException<BridgeSubmissionResult>(error),
                _ => throw new InvalidOperationException("Unexpected fake outcome."),
            };
        }
    }

    private sealed class AlwaysUnavailableClient : IBridgeRequestClient
    {
        public int CallCount { get; private set; }

        public Task<BridgeSubmissionResult> SubmitAsync(ConversionRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return Task.FromException<BridgeSubmissionResult>(new BridgeHostUnavailableException("offline"));
        }
    }
}
